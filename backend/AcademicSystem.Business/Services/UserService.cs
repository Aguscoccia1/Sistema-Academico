using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Business.Services;

public class UserService(AcademicSystemContext context) : IUserService
{
    private readonly PasswordHasher<User> passwordHasher = new();

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

    public Task<bool> RoleExistsAsync(int roleId, CancellationToken ct) =>
        context.Roles.AnyAsync(r => r.Id == roleId, ct);

    public Task<string?> GetRoleNameAsync(int roleId, CancellationToken ct) =>
        context.Roles.Where(r => r.Id == roleId).Select(r => r.Name).SingleOrDefaultAsync(ct);

    public Task<bool> CuilOrEmailExistsAsync(string cuil, string email, CancellationToken ct) =>
        context.Users.AnyAsync(u => u.Cuil == cuil || u.Email == email, ct);

    public Task<bool> CuilOrEmailExistsAsync(string cuil, string email, int excludeId, CancellationToken ct) =>
        context.Users.AnyAsync(u => u.Id != excludeId && (u.Cuil == cuil || u.Email == email), ct);

    public Task<bool> HasStudentProfileAsync(int userId, CancellationToken ct) =>
        context.Students.AnyAsync(s => s.UserId == userId, ct);

    public Task<bool> HasSecretaryProfileAsync(int userId, CancellationToken ct) =>
        context.AcademicSecretaries.AnyAsync(s => s.UserId == userId, ct);

    public async Task<bool> IsLastActiveSecretaryAsync(int userId, CancellationToken ct)
    {
        var user = await context.Users.Where(u => u.Id == userId)
            .Select(u => new { u.IsActive, RoleName = u.Role.Name })
            .SingleOrDefaultAsync(ct);

        if (user is null || !user.IsActive || user.RoleName != RoleNames.AcademicSecretary)
        {
            return false;
        }

        var otherActive = await context.Users.AnyAsync(u => u.Id != userId && u.IsActive
            && u.Role.Name == RoleNames.AcademicSecretary, ct);

        return !otherActive;
    }

    public async Task<UserResponseDto> CreateAsync(UserRequestDto input, CancellationToken ct)
    {
        User user = new User
        {
            FirstName = input.FirstName.Trim(),
            LastName = input.LastName.Trim(),
            Cuil = input.Cuil.Trim(),
            BirthDate = input.BirthDate,
            Address = Clean(input.Address),
            RoleId = input.RoleId,
            Email = input.Email.Trim(),
            Phone = Clean(input.Phone),
            EmergencyContactPhone = Clean(input.EmergencyContactPhone),
            IsActive = true
        };

        user.PasswordHash = passwordHasher.HashPassword(user, input.Password!);

        context.Users.Add(user);
        await context.SaveChangesAsync(ct);

        string roleName = await GetRoleNameAsync(user.RoleId, ct) ?? "";

        return ToDto(user, roleName);
    }

    public async Task<UserResponseDto?> UpdateAsync(int id, UserRequestDto input, CancellationToken ct)
    {
        User? user = await context.Users.SingleOrDefaultAsync(u => u.Id == id, ct);

        if (user is null)
        {
            return null;
        }

        user.FirstName = input.FirstName.Trim();
        user.LastName = input.LastName.Trim();
        user.Cuil = input.Cuil.Trim();
        user.BirthDate = input.BirthDate;
        user.Address = Clean(input.Address);
        user.RoleId = input.RoleId;
        user.Email = input.Email.Trim();
        user.Phone = Clean(input.Phone);
        user.EmergencyContactPhone = Clean(input.EmergencyContactPhone);
        user.IsActive = input.IsActive;

        if (!string.IsNullOrWhiteSpace(input.Password))
        {
            user.PasswordHash = passwordHasher.HashPassword(user, input.Password);
        }

        await context.SaveChangesAsync(ct);

        string roleName = await GetRoleNameAsync(user.RoleId, ct) ?? "";

        return ToDto(user, roleName);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        User? user = await context.Users.SingleOrDefaultAsync(u => u.Id == id, ct);

        if (user is null)
        {
            return false;
        }

        user.IsActive = false;
        await context.SaveChangesAsync(ct);

        return true;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static UserResponseDto ToDto(User u, string roleName) =>
        new(u.Id, u.FirstName, u.LastName, u.Cuil, u.BirthDate, u.Address, u.RoleId, roleName,
            u.Email, u.Phone, u.EmergencyContactPhone, u.IsActive);
}
