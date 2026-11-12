using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Repositories;

public class AcademicSecretaryRepository(AcademicSystemContext context) : IAcademicSecretaryRepository
{
    public Task<List<AcademicSecretaryResponseDto>> GetAllAsync(CancellationToken ct) =>
        context.AcademicSecretaries.AsNoTracking()
            .OrderBy(s => s.Id)
            .Select(s => new AcademicSecretaryResponseDto(s.Id, s.UserId, s.TitlesJson))
            .ToListAsync(ct);

    public Task<AcademicSecretaryResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        context.AcademicSecretaries.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new AcademicSecretaryResponseDto(s.Id, s.UserId, s.TitlesJson))
            .SingleOrDefaultAsync(ct);

    public Task<AcademicSecretary?> FindAsync(int id, CancellationToken ct) =>
        context.AcademicSecretaries.SingleOrDefaultAsync(s => s.Id == id, ct);

    public Task<bool> ExistsForUserAsync(int userId, CancellationToken ct) =>
        context.AcademicSecretaries.AnyAsync(s => s.UserId == userId, ct);

    public void Add(AcademicSecretary secretary) => context.AcademicSecretaries.Add(secretary);

    public void Remove(AcademicSecretary secretary) => context.AcademicSecretaries.Remove(secretary);

    public Task SaveChangesAsync(CancellationToken ct) => context.SaveChangesAsync(ct);
}
