using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public interface IRoleService
{
    Task<List<RoleResponseDto>> GetAllAsync(CancellationToken ct);
}
