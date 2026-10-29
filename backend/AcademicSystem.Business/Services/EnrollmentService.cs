using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Business.Services;

public sealed class EnrollmentService(
    AcademicSystemContext context,
    IPrerequisiteValidationService prerequisiteValidationService) : IEnrollmentService
{
    private const string ApprovedStatus = "Aprobado";

    public async Task<AvailableSubjectsResponseDto?> GetAvailableAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        int? studentId = await context.Students
            .AsNoTracking()
            .Where(student => student.UserId == userId)
            .Select(student => (int?)student.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (studentId is null)
        {
            return null;
        }

        var rows = await context.StudyPlanSubjects
            .AsNoTracking()
            .Where(planSubject =>
                planSubject.StudyPlan.IsActive &&
                (planSubject.Subject.TeacherSubjects.Any(teacherSubject =>
                    teacherSubject.Section.Trim().ToUpper() == "A"
                    || teacherSubject.Section.Trim().ToUpper() == "B")
                || planSubject.Subject.SubjectSchedules.Any(schedule =>
                    schedule.Section.Trim().ToUpper() == "A"
                    || schedule.Section.Trim().ToUpper() == "B")
                || planSubject.Subject.StudentSubjects.Any(enrollment =>
                    enrollment.Section.Trim().ToUpper() == "A"
                    || enrollment.Section.Trim().ToUpper() == "B")))
            .Select(planSubject => new
            {
                planSubject.StudyPlanId,
                planSubject.Subject.Id,
                planSubject.Subject.Name,
                planSubject.CourseYear,
                planSubject.Term
            })
            .Distinct()
            .OrderBy(subject => subject.CourseYear)
            .ThenBy(subject => subject.Name)
            .ToListAsync(cancellationToken);

        var subjects = rows
            .Select(row => new AvailableSubjectDto(
                row.StudyPlanId,
                row.Id,
                row.Id,
                row.Name,
                row.CourseYear,
                row.Term))
            .ToList();

        return new AvailableSubjectsResponseDto(
            subjects.Count == 0
                ? "No hay materias disponibles para inscripción en el período vigente."
                : null,
            subjects);
    }

    public async Task<IReadOnlyList<StudentEnrollmentDto>?> GetStudentEnrollmentsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        int? studentId = await context.Students
            .AsNoTracking()
            .Where(student => student.UserId == userId)
            .Select(student => (int?)student.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (studentId is null)
        {
            return null;
        }

        var activePlanSubjects = context.StudyPlanSubjects
            .AsNoTracking()
            .Where(planSubject => planSubject.StudyPlan.IsActive);

        return await context.StudentSubjects
            .AsNoTracking()
            .Where(enrollment => enrollment.StudentId == studentId.Value)
            .OrderBy(enrollment => enrollment.Subject.Name)
            .ThenBy(enrollment => enrollment.Section)
            .Select(enrollment => new StudentEnrollmentDto(
                enrollment.Id,
                enrollment.SubjectId,
                enrollment.Subject.Name,
                enrollment.Section,
                activePlanSubjects
                    .Where(planSubject => planSubject.SubjectId == enrollment.SubjectId)
                    .OrderBy(planSubject => planSubject.StudyPlanId)
                    .ThenBy(planSubject => planSubject.Id)
                    .Select(planSubject => (int?)planSubject.CourseYear)
                    .FirstOrDefault(),
                activePlanSubjects
                    .Where(planSubject => planSubject.SubjectId == enrollment.SubjectId)
                    .OrderBy(planSubject => planSubject.StudyPlanId)
                    .ThenBy(planSubject => planSubject.Id)
                    .Select(planSubject => planSubject.Term)
                    .FirstOrDefault(),
                enrollment.CourseRecord == null ? null : enrollment.CourseRecord.FinalStatus))
            .ToListAsync(cancellationToken);
    }

    public async Task<EnrollmentResult> EnrollAsync(
        int userId,
        StudentSubjectRequestDto request,
        CancellationToken cancellationToken = default)
    {
        int? studentId = await context.Students
            .AsNoTracking()
            .Where(student => student.UserId == userId)
            .Select(student => (int?)student.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (studentId is null)
        {
            return new EnrollmentResult(
                EnrollmentResultStatus.NotFound,
                Message: "No se encontró el perfil de estudiante del usuario autenticado.");
        }

        var existingEnrollment = await context.StudentSubjects
            .AsNoTracking()
            .Where(item => item.StudentId == studentId.Value && item.SubjectId == request.SubjectId)
            .Select(item => new
            {
                FinalStatus = item.CourseRecord == null ? null : item.CourseRecord.FinalStatus
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (existingEnrollment is not null)
        {
            return new EnrollmentResult(
                EnrollmentResultStatus.Conflict,
                Message: GetExistingEnrollmentMessage(existingEnrollment.FinalStatus));
        }

        var validation = await prerequisiteValidationService.ValidateAsync(
            userId,
            new PrerequisiteValidationRequestDto
            {
                StudyPlanId = request.StudyPlanId,
                SubjectId = request.SubjectId
            },
            cancellationToken);

        if (validation is null)
        {
            return new EnrollmentResult(
                EnrollmentResultStatus.NotFound,
                Message: "No existe el plan, la materia o el perfil de estudiante indicado.");
        }

        if (!validation.CanEnroll && validation.MissingPrerequisites.Count == 0)
        {
            return new EnrollmentResult(
                EnrollmentResultStatus.InvalidRequest,
                Message: validation.Message);
        }

        if (!validation.CanEnroll)
        {
            return new EnrollmentResult(
                EnrollmentResultStatus.PrerequisitesNotMet,
                Message: validation.Message,
                Validation: validation);
        }

        await using var transaction = await context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable,
            cancellationToken);

        var lockResource = $"comisiones-asignacion-materia-{request.SubjectId}";
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            DECLARE @lockResult int;
            EXEC @lockResult = sys.sp_getapplock
                @Resource = {lockResource},
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = -1;
            IF @lockResult < 0
            BEGIN
                ;THROW 51001, 'No se pudo obtener el bloqueo para asignar comisiones.', 1;
            END;
            """,
            cancellationToken);

        // Recheck after taking the same subject lock used by E2.
        var duplicateEnrollment = await context.StudentSubjects
            .AsNoTracking()
            .Where(item => item.StudentId == studentId.Value && item.SubjectId == request.SubjectId)
            .Select(item => new
            {
                FinalStatus = item.CourseRecord == null ? null : item.CourseRecord.FinalStatus
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (duplicateEnrollment is not null)
        {
            return new EnrollmentResult(
                EnrollmentResultStatus.Conflict,
                Message: GetExistingEnrollmentMessage(duplicateEnrollment.FinalStatus));
        }

        var configuredSections = await context.TeacherSubjects
            .AsNoTracking()
            .Where(assignment => assignment.SubjectId == request.SubjectId)
            .Select(assignment => assignment.Section)
            .Concat(context.SubjectSchedules
                .AsNoTracking()
                .Where(schedule => schedule.SubjectId == request.SubjectId)
                .Select(schedule => schedule.Section))
            .ToListAsync(cancellationToken);

        var existingSections = await context.StudentSubjects
            .AsNoTracking()
            .Where(enrollment => enrollment.SubjectId == request.SubjectId)
            .Select(enrollment => enrollment.Section)
            .ToListAsync(cancellationToken);

        var sections = configuredSections
            .Concat(existingSections)
            .Select(NormalizeSection)
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .OrderBy(section => section)
            .ToList();

        if (sections.Count == 0)
        {
            return new EnrollmentResult(
                EnrollmentResultStatus.InvalidRequest,
                Message: "La materia no tiene comisiones A o B disponibles.");
        }

        var sectionCounts = sections.ToDictionary(
            section => section,
            _ => 0,
            StringComparer.Ordinal);

        foreach (var existingSection in existingSections)
        {
            var normalizedSection = NormalizeSection(existingSection);
            if (normalizedSection is not null && sectionCounts.ContainsKey(normalizedSection))
            {
                sectionCounts[normalizedSection]++;
            }
        }

        int leastEnrollmentCount = sectionCounts.Values.Min();
        var leastPopulatedSections = sections
            .Where(section => sectionCounts[section] == leastEnrollmentCount)
            .ToArray();
        string section = leastPopulatedSections[Random.Shared.Next(leastPopulatedSections.Length)];

        var enrollment = new StudentSubject
        {
            StudentId = studentId.Value,
            SubjectId = request.SubjectId,
            Section = section
        };

        context.StudentSubjects.Add(enrollment);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            return new EnrollmentResult(
                EnrollmentResultStatus.Conflict,
                Message: "Ya existe una inscripción o un registro de cursada para esta materia.");
        }

        return new EnrollmentResult(
            EnrollmentResultStatus.Created,
            Enrollment: new StudentSubjectResponseDto
            {
                Id = enrollment.Id,
                StudentId = enrollment.StudentId,
                SubjectId = enrollment.SubjectId,
                Section = enrollment.Section
            });
    }

    private static string? NormalizeSection(string? section)
    {
        var normalizedSection = section?.Trim().ToUpperInvariant();
        return normalizedSection is "A" or "B" ? normalizedSection : null;
    }

    private static string GetExistingEnrollmentMessage(string? finalStatus)
    {
        if (finalStatus is null)
        {
            return "Ya estás inscripto en esta materia.";
        }

        if (finalStatus == ApprovedStatus)
        {
            return "Ya aprobaste esta materia.";
        }

        return "Ya cursaste esta materia. Volver a cursarla no está disponible en el sistema actual; consultá con Secretaría.";
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException sqlException
            && (sqlException.Number == 2601 || sqlException.Number == 2627);
    }
}
