using AcademicSystem.Data.Context;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;
 
namespace AcademicSystem.Data.Repositories;
 
public class SubjectRepository : ISubjectRepository
{
    private readonly AcademicSystemContext _context;
 
    public SubjectRepository(AcademicSystemContext context)
    {
        _context = context;
    }
 
    public async Task<List<Subject>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Subjects
            .AsNoTracking()
            .ToListAsync(ct);
    }
 
    public async Task<Subject?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Subjects.FirstOrDefaultAsync(s => s.Id == id, ct);
    }
 
    public async Task<bool> ExistsAsync(int id, CancellationToken ct)
    {
        return await _context.Subjects.AnyAsync(s => s.Id == id, ct);
    }
 
    public void Add(Subject subject)
    {
        _context.Subjects.Add(subject);
    }
 
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}