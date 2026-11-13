using AcademicSystem.Entities.Models;
 
namespace AcademicSystem.Data.Repositories;
 
public interface IStudyPlanRepository
{
    Task<List<StudyPlan>> GetAllAsync(CancellationToken ct);
    Task<StudyPlan?> GetByIdAsync(int id, CancellationToken ct);
    Task<StudyPlan?> GetByIdWithSubjectsAsync(int id, CancellationToken ct);
    Task<bool> ExistsAsync(int id, CancellationToken ct);
    Task<bool> ExistsByPlanCodeAsync(string planCode, int? excludeId, CancellationToken ct);
    Task<bool> ExistsSubjectInPlanAsync(int studyPlanId, int subjectId, CancellationToken ct);
    Task<StudyPlanSubject?> GetPlanSubjectAsync(int id, CancellationToken ct);
    Task<bool> RemoveSubjectFromPlanAsync(int studyPlanId, int subjectId, CancellationToken ct);
 
    void Add(StudyPlan studyPlan);
    void AddSubject(StudyPlanSubject studyPlanSubject);
 
    Task SaveChangesAsync(CancellationToken ct);
}