using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;

namespace AcademicSystem.Data.Repositories;

public interface IAcademicSecretaryRepository
{
    Task<List<AcademicSecretaryResponseDto>> GetAllAsync(CancellationToken ct);

    Task<AcademicSecretaryResponseDto?> GetByIdAsync(int id, CancellationToken ct);

    Task<AcademicSecretary?> FindAsync(int id, CancellationToken ct);

    Task<bool> ExistsForUserAsync(int userId, CancellationToken ct);

    void Add(AcademicSecretary secretary);

    void Remove(AcademicSecretary secretary);

    Task SaveChangesAsync(CancellationToken ct);
}
