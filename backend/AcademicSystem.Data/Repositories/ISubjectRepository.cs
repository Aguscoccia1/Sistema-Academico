using AcademicSystem.Entities.Models;
 
namespace AcademicSystem.Data.Repositories;
 
public interface ISubjectRepository
{
    Task<List<Subject>> GetAllAsync(CancellationToken ct);
    Task<Subject?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> ExistsAsync(int id, CancellationToken ct);
 
    void Add(Subject subject);
 
    Task SaveChangesAsync(CancellationToken ct);
}