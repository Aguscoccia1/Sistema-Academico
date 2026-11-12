using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Data.Repositories;

public interface IRoleRepository
{
    Task<List<RoleResponseDto>> GetAllAsync(CancellationToken ct);
}
