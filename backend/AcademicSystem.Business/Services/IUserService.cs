using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public interface IUserService
{
    Task<List<UserResponseDto>> GetAllAsync(CancellationToken ct);
    Task<UserResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> RoleExistsAsync(int roleId, CancellationToken ct);
    Task<string?> GetRoleNameAsync(int roleId, CancellationToken ct);
    Task<bool> CuilOrEmailExistsAsync(string cuil, string email, CancellationToken ct);
    Task<bool> CuilOrEmailExistsAsync(string cuil, string email, int excludeId, CancellationToken ct);
    Task<bool> HasStudentProfileAsync(int userId, CancellationToken ct);
    Task<bool> HasSecretaryProfileAsync(int userId, CancellationToken ct);
    Task<bool> IsLastActiveSecretaryAsync(int userId, CancellationToken ct);
    Task<UserResponseDto> CreateAsync(UserRequestDto input, CancellationToken ct);
    Task<UserResponseDto?> UpdateAsync(int id, UserRequestDto input, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
