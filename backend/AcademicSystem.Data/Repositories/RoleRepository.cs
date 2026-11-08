using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Repositories;

public class RoleRepository(AcademicSystemContext context) : IRoleRepository
{
    public Task<List<RoleResponseDto>> GetAllAsync(CancellationToken ct) =>
        context.Roles.AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleResponseDto(r.Id, r.Name))
            .ToListAsync(ct);
}
