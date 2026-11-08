using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;
 
namespace AcademicSystem.Data.Repositories;
 
public class PrerequisiteRepository : IPrerequisiteRepository
{
    private readonly AcademicSystemContext _context;
 
    public PrerequisiteRepository(AcademicSystemContext context)
    {
        _context = context;
    }
 
    public async Task<List<PrerequisiteItemDto>> GetByPlanAsync(int studyPlanId, CancellationToken ct)
    {
        return await _context.Prerequisites
            .AsNoTracking()
            .Where(p => p.StudyPlanId == studyPlanId)
            .OrderBy(p => p.SubjectId)
            .ThenBy(p => p.PrerequisiteSubjectId)
            .Select(p => new PrerequisiteItemDto(p.SubjectId, p.PrerequisiteSubjectId, p.PrerequisiteSubject.Name))
            .ToListAsync(ct);
    }
 
    public async Task<List<int>> GetSubjectIdsInPlanAsync(int studyPlanId, CancellationToken ct)
    {
        return await _context.StudyPlanSubjects
            .AsNoTracking()
            .Where(sps => sps.StudyPlanId == studyPlanId)
            .Select(sps => sps.SubjectId)
            .ToListAsync(ct);
    }
 
    public async Task<List<(int SubjectId, int PrerequisiteSubjectId)>> GetPairsByPlanAsync(int studyPlanId, CancellationToken ct)
    {
        var pairs = await _context.Prerequisites
            .AsNoTracking()
            .Where(p => p.StudyPlanId == studyPlanId)
            .Select(p => new { p.SubjectId, p.PrerequisiteSubjectId })
            .ToListAsync(ct);
 
        return pairs
            .Select(pair => (pair.SubjectId, pair.PrerequisiteSubjectId))
            .ToList();
    }
 
    public async Task ReplaceAsync(int studyPlanId, int subjectId, IReadOnlyList<int> prerequisiteSubjectIds, CancellationToken ct)
    {
        List<Prerequisite> current = await _context.Prerequisites
            .Where(p => p.StudyPlanId == studyPlanId && p.SubjectId == subjectId)
            .ToListAsync(ct);
 
        _context.Prerequisites.RemoveRange(current.Where(p => !prerequisiteSubjectIds.Contains(p.PrerequisiteSubjectId)));
 
        IEnumerable<int> toAdd = prerequisiteSubjectIds.Where(id => current.All(p => p.PrerequisiteSubjectId != id));
 
        foreach (int prerequisiteSubjectId in toAdd)
        {
            _context.Prerequisites.Add(new Prerequisite
            {
                StudyPlanId = studyPlanId,
                SubjectId = subjectId,
                PrerequisiteSubjectId = prerequisiteSubjectId
            });
        }
 
        await _context.SaveChangesAsync(ct);
    }
}