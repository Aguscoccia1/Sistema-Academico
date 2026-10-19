using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public interface IUserService
{
    Task<List<UserResponseDto>> GetAllAsync(CancellationToken ct);
    Task<UserResponseDto?> GetByIdAsync(int id, CancellationToken ct);
}
