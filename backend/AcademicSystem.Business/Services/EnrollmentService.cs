using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public sealed class EnrollmentService(
    IEnrollmentRepository enrollmentRepository,
    IPrerequisiteValidationService prerequisiteValidationService) : IEnrollmentService
{
    private const string ApprovedStatus = "Aprobado";

    public async Task<AvailableSubjectsResponseDto?> GetAvailableAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        int? studentId = await enrollmentRepository.GetStudentIdByUserIdAsync(userId, cancellationToken);
        if (studentId is null)
        {
            return null;
        }

        var subjects = await enrollmentRepository.GetAvailableSubjectsAsync(cancellationToken);
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
        int? studentId = await enrollmentRepository.GetStudentIdByUserIdAsync(userId, cancellationToken);
        if (studentId is null)
        {
            return null;
        }

        return await enrollmentRepository.GetStudentEnrollmentsAsync(studentId.Value, cancellationToken);
    }

    public async Task<EnrollmentResult> EnrollAsync(
        int userId,
        StudentSubjectRequestDto request,
        CancellationToken cancellationToken = default)
    {
        int? studentId = await enrollmentRepository.GetStudentIdByUserIdAsync(userId, cancellationToken);
        if (studentId is null)
        {
            return new EnrollmentResult(
                EnrollmentResultStatus.NotFound,
                Message: "No se encontró el perfil de estudiante del usuario autenticado.");
        }

        var existingEnrollment = await enrollmentRepository.GetEnrollmentAsync(
            studentId.Value,
            request.SubjectId,
            cancellationToken);

        if (existingEnrollment is not null)
        {
            return new EnrollmentResult(
                EnrollmentResultStatus.Conflict,
                Message: GetExistingEnrollmentMessage(existingEnrollment.CourseRecord?.FinalStatus));
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

        var creationResult = await enrollmentRepository.CreateEnrollmentAsync(
            studentId.Value,
            request.SubjectId,
            cancellationToken);

        if (creationResult.Status == EnrollmentCreationStatus.AlreadyExists)
        {
            return new EnrollmentResult(
                EnrollmentResultStatus.Conflict,
                Message: GetExistingEnrollmentMessage(creationResult.ExistingFinalStatus));
        }

        if (creationResult.Status == EnrollmentCreationStatus.NoAvailableSections)
        {
            return new EnrollmentResult(
                EnrollmentResultStatus.InvalidRequest,
                Message: "La materia no tiene comisiones A o B disponibles.");
        }

        var enrollment = creationResult.Enrollment
            ?? throw new InvalidOperationException("El repositorio no devolvió la inscripción creada.");

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
}
