using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public interface IEnrollmentService
{
    Task<AvailableSubjectsResponseDto?> GetAvailableAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentEnrollmentDto>?> GetStudentEnrollmentsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<EnrollmentResult> EnrollAsync(
        int userId,
        StudentSubjectRequestDto request,
        CancellationToken cancellationToken = default);
}
