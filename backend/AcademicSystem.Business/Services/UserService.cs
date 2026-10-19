using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Business.Services;

public class UserService(AcademicSystemContext context) : IUserService
{
    public Task<List<UserResponseDto>> GetAllAsync(CancellationToken ct) =>
        context.Users.AsNoTracking()
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .Select(u => new UserResponseDto(u.Id, u.FirstName, u.LastName, u.Cuil, u.BirthDate, u.Address,
                u.RoleId, u.Role.Name, u.Email, u.Phone, u.EmergencyContactPhone, u.IsActive))
            .ToListAsync(ct);

    public Task<UserResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        context.Users.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserResponseDto(u.Id, u.FirstName, u.LastName, u.Cuil, u.BirthDate, u.Address,
                u.RoleId, u.Role.Name, u.Email, u.Phone, u.EmergencyContactPhone, u.IsActive))
            .SingleOrDefaultAsync(ct);
}
