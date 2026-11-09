using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public class RoleService(IRoleRepository roleRepository) : IRoleService
{
    public Task<List<RoleResponseDto>> GetAllAsync(CancellationToken ct) => roleRepository.GetAllAsync(ct);
}
