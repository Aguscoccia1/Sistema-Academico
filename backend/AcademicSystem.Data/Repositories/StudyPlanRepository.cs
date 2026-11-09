using AcademicSystem.Data.Context;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;
 
namespace AcademicSystem.Data.Repositories;
 
public class StudyPlanRepository : IStudyPlanRepository
{
    private readonly AcademicSystemContext _context;
 
    public StudyPlanRepository(AcademicSystemContext context)
    {
        _context = context;
    }
 
    public async Task<List<StudyPlan>> GetAllAsync(CancellationToken ct)
    {
        return await _context.StudyPlans
            .AsNoTracking()
            .ToListAsync(ct);
    }
 
    // con seguimiento de cambios: se usa para modificar el plan
    public async Task<StudyPlan?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.StudyPlans.FirstOrDefaultAsync(sp => sp.Id == id, ct);
    }
 
    // solo lectura: trae el plan con sus materias
    public async Task<StudyPlan?> GetByIdWithSubjectsAsync(int id, CancellationToken ct)
    {
        return await _context.StudyPlans
            .AsNoTracking()
            .Include(sp => sp.StudyPlanSubjects)
                .ThenInclude(sps => sps.Subject)
            .FirstOrDefaultAsync(sp => sp.Id == id, ct);
    }
 
    public async Task<bool> ExistsAsync(int id, CancellationToken ct)
    {
        return await _context.StudyPlans.AnyAsync(sp => sp.Id == id, ct);
    }
 
    // excludeId permite ignorar el propio plan cuando se está editando (null = no excluir ninguno)
    public async Task<bool> ExistsByPlanCodeAsync(string planCode, int? excludeId, CancellationToken ct)
    {
        IQueryable<StudyPlan> plans = _context.StudyPlans.Where(sp => sp.PlanCode == planCode);
 
        if (excludeId != null)
        {
            plans = plans.Where(sp => sp.Id != excludeId.Value);
        }
 
        return await plans.AnyAsync(ct);
    }
 
    public async Task<bool> ExistsSubjectInPlanAsync(int studyPlanId, int subjectId, CancellationToken ct)
    {
        return await _context.StudyPlanSubjects
            .AnyAsync(sps => sps.StudyPlanId == studyPlanId && sps.SubjectId == subjectId, ct);
    }
 
    public async Task<StudyPlanSubject?> GetPlanSubjectAsync(int id, CancellationToken ct)
    {
        return await _context.StudyPlanSubjects
            .AsNoTracking()
            .Include(sps => sps.Subject)
            .FirstOrDefaultAsync(sps => sps.Id == id, ct);
    }
 
    public void Add(StudyPlan studyPlan)
    {
        _context.StudyPlans.Add(studyPlan);
    }
 
    public void AddSubject(StudyPlanSubject studyPlanSubject)
    {
        _context.StudyPlanSubjects.Add(studyPlanSubject);
    }
 
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> RemoveSubjectFromPlanAsync(
    int studyPlanId,
    int subjectId,
    CancellationToken ct)
{
    StudyPlanSubject? planSubject = await _context.StudyPlanSubjects
        .Include(item => item.AssessmentInstances)
        .FirstOrDefaultAsync(
            item => item.StudyPlanId == studyPlanId && item.SubjectId == subjectId,
            ct);

    if (planSubject == null)
    {
        return false;
    }

    if (planSubject.AssessmentInstances.Count > 0)
    {
        throw new InvalidOperationException(
            "No se puede quitar la materia porque tiene evaluaciones asociadas.");
    }

    var prerequisites = await _context.Prerequisites
        .Where(item =>
            item.StudyPlanId == studyPlanId
            && (item.SubjectId == subjectId || item.PrerequisiteSubjectId == subjectId))
        .ToListAsync(ct);

    _context.Prerequisites.RemoveRange(prerequisites);
    _context.StudyPlanSubjects.Remove(planSubject);

    await _context.SaveChangesAsync(ct);
    return true;
}
}