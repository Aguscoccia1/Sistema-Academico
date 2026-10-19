using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public interface IPrerequisiteValidationService
{
    Task<PrerequisiteValidationResponseDto?> ValidateAsync(
        int userId,
        PrerequisiteValidationRequestDto request,
        CancellationToken cancellationToken = default);
}
