using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public interface IAcademicSecretaryService
{
    Task<List<AcademicSecretaryResponseDto>> GetAllAsync(CancellationToken ct);

    Task<AcademicSecretaryResponseDto?> GetByIdAsync(int id, CancellationToken ct);

    Task<ProfileResult<AcademicSecretaryResponseDto>> CreateAsync(AcademicSecretaryRequestDto input, CancellationToken ct);

    Task<ProfileResult<AcademicSecretaryResponseDto>> UpdateAsync(int id, AcademicSecretaryRequestDto input, CancellationToken ct);

    Task<ProfileResult<AcademicSecretaryResponseDto>> DeleteAsync(int id, CancellationToken ct);
}
