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

    public async Task<AvailableSubjectsResponseDto> GetAvailableAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await context.StudyPlanSubjects
            .AsNoTracking()
            .Where(planSubject =>
                planSubject.StudyPlan.IsActive &&
                planSubject.Subject.TeacherSubjects.Any())
            .Select(planSubject => new
            {
                planSubject.StudyPlanId,
                planSubject.Subject.Id,
                planSubject.Subject.Name,
                planSubject.CourseYear
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
                row.CourseYear))
            .ToList();

        return new AvailableSubjectsResponseDto(
            subjects.Count == 0
                ? "No hay materias disponibles para inscripción en el período vigente."
                : null,
            subjects);
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

        bool sectionIsAvailable = await context.StudyPlanSubjects
            .AsNoTracking()
            .AnyAsync(item =>
                item.StudyPlanId == request.StudyPlanId
                && item.SubjectId == request.SubjectId
                && item.StudyPlan.IsActive
                && item.Subject.TeacherSubjects.Any(teacherSubject =>
                    teacherSubject.Section == request.Section),
                cancellationToken);

        if (!sectionIsAvailable)
        {
            return new EnrollmentResult(
                EnrollmentResultStatus.InvalidRequest,
                Message: "La materia o comisión seleccionada no está disponible para inscripción.");
        }

        var enrollment = new StudentSubject
        {
            StudentId = studentId.Value,
            SubjectId = request.SubjectId,
            Section = request.Section
        };

        context.StudentSubjects.Add(enrollment);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
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
