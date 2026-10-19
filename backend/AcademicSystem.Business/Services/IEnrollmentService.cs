using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public interface IEnrollmentService
{
    Task<AvailableSubjectsResponseDto> GetAvailableAsync(CancellationToken cancellationToken = default);
}
