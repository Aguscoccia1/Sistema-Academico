using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.AspNetCore.Identity;

namespace AcademicSystem.Business.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    private readonly PasswordHasher<User> passwordHasher = new();

    public Task<List<UserResponseDto>> GetAllAsync(CancellationToken ct) => userRepository.GetAllAsync(ct);

    public Task<UserResponseDto?> GetByIdAsync(int id, CancellationToken ct) => userRepository.GetByIdAsync(id, ct);

    public Task<bool> RoleExistsAsync(int roleId, CancellationToken ct) => userRepository.RoleExistsAsync(roleId, ct);

    public Task<string?> GetRoleNameAsync(int roleId, CancellationToken ct) => userRepository.GetRoleNameAsync(roleId, ct);

    public Task<bool> CuilOrEmailExistsAsync(string cuil, string email, CancellationToken ct) =>
        userRepository.CuilOrEmailExistsAsync(cuil, email, ct);

    public Task<bool> CuilOrEmailExistsAsync(string cuil, string email, int excludeId, CancellationToken ct) =>
        userRepository.CuilOrEmailExistsAsync(cuil, email, excludeId, ct);

    public Task<bool> HasStudentProfileAsync(int userId, CancellationToken ct) => userRepository.HasStudentProfileAsync(userId, ct);

    public Task<bool> HasSecretaryProfileAsync(int userId, CancellationToken ct) => userRepository.HasSecretaryProfileAsync(userId, ct);

    public async Task<bool> IsLastActiveSecretaryAsync(int userId, CancellationToken ct)
    {
        if (!await userRepository.IsActiveWithRoleAsync(userId, RoleNames.AcademicSecretary, ct))
        {
            return false;
        }

        return !await userRepository.OtherActiveWithRoleExistsAsync(userId, RoleNames.AcademicSecretary, ct);
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

        userRepository.Add(user);
        await userRepository.SaveChangesAsync(ct);

        string roleName = await GetRoleNameAsync(user.RoleId, ct) ?? "";

        return ToDto(user, roleName);
    }

    public async Task<UserResponseDto?> UpdateAsync(int id, UserRequestDto input, CancellationToken ct)
    {
        User? user = await userRepository.FindAsync(id, ct);

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

        await userRepository.SaveChangesAsync(ct);

        string roleName = await GetRoleNameAsync(user.RoleId, ct) ?? "";

        return ToDto(user, roleName);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        User? user = await userRepository.FindAsync(id, ct);

        if (user is null)
        {
            return false;
        }

        user.IsActive = false;
        await userRepository.SaveChangesAsync(ct);

        return true;
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static UserResponseDto ToDto(User u, string roleName) =>
        new(u.Id, u.FirstName, u.LastName, u.Cuil, u.BirthDate, u.Address, u.RoleId, roleName,
            u.Email, u.Phone, u.EmergencyContactPhone, u.IsActive);
}
