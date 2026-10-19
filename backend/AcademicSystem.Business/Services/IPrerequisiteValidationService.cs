using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public interface IPrerequisiteValidationService
{
    Task<PrerequisiteValidationResponseDto?> ValidateAsync(
        PrerequisiteValidationRequestDto request,
        CancellationToken cancellationToken = default);
}
