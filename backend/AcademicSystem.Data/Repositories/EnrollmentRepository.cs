using System.Data;
using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Repositories;

public sealed class EnrollmentRepository(AcademicSystemContext context) : IEnrollmentRepository
{
    public async Task<int?> GetStudentIdByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await context.Students
            .AsNoTracking()
            .Where(student => student.UserId == userId)
            .Select(student => (int?)student.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AvailableSubjectDto>> GetAvailableSubjectsAsync(
        CancellationToken cancellationToken = default)
    {
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

        return rows
            .Select(row => new AvailableSubjectDto(
                row.StudyPlanId,
                row.Id,
                row.Id,
                row.Name,
                row.CourseYear,
                row.Term))
            .ToList();
    }

    public async Task<IReadOnlyList<StudentEnrollmentDto>> GetStudentEnrollmentsAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        var activePlanSubjects = context.StudyPlanSubjects
            .AsNoTracking()
            .Where(planSubject => planSubject.StudyPlan.IsActive);

        return await context.StudentSubjects
            .AsNoTracking()
            .Where(enrollment => enrollment.StudentId == studentId)
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

    public async Task<StudentSubject?> GetEnrollmentAsync(
        int studentId,
        int subjectId,
        CancellationToken cancellationToken = default)
    {
        return await context.StudentSubjects
            .AsNoTracking()
            .Include(enrollment => enrollment.CourseRecord)
            .SingleOrDefaultAsync(
                enrollment => enrollment.StudentId == studentId && enrollment.SubjectId == subjectId,
                cancellationToken);
    }

    public async Task<EnrollmentCreationResult> CreateEnrollmentAsync(
        int studentId,
        int subjectId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var lockResource = $"comisiones-asignacion-materia-{subjectId}";
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

        var existingEnrollment = await context.StudentSubjects
            .AsNoTracking()
            .Include(enrollment => enrollment.CourseRecord)
            .SingleOrDefaultAsync(
                enrollment => enrollment.StudentId == studentId && enrollment.SubjectId == subjectId,
                cancellationToken);

        if (existingEnrollment is not null)
        {
            return new EnrollmentCreationResult(
                EnrollmentCreationStatus.AlreadyExists,
                ExistingFinalStatus: existingEnrollment.CourseRecord?.FinalStatus);
        }

        var configuredSections = await context.TeacherSubjects
            .AsNoTracking()
            .Where(assignment => assignment.SubjectId == subjectId)
            .Select(assignment => assignment.Section)
            .Concat(context.SubjectSchedules
                .AsNoTracking()
                .Where(schedule => schedule.SubjectId == subjectId)
                .Select(schedule => schedule.Section))
            .ToListAsync(cancellationToken);

        var existingSections = await context.StudentSubjects
            .AsNoTracking()
            .Where(enrollment => enrollment.SubjectId == subjectId)
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
            return new EnrollmentCreationResult(EnrollmentCreationStatus.NoAvailableSections);
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
        string section = leastPopulatedSections[
            Random.Shared.Next(leastPopulatedSections.Length)];

        var enrollment = new StudentSubject
        {
            StudentId = studentId,
            SubjectId = subjectId,
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
            return new EnrollmentCreationResult(EnrollmentCreationStatus.AlreadyExists);
        }

        return new EnrollmentCreationResult(EnrollmentCreationStatus.Created, enrollment);
    }

    private static string? NormalizeSection(string? section)
    {
        var normalizedSection = section?.Trim().ToUpperInvariant();
        return normalizedSection is "A" or "B" ? normalizedSection : null;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException sqlException
            && (sqlException.Number == 2601 || sqlException.Number == 2627);
    }
}
