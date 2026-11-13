using AcademicSystem.Entities.DTOs;
 
namespace AcademicSystem.Data.Repositories;
 
public interface IPrerequisiteRepository
{
    Task<List<PrerequisiteItemDto>> GetByPlanAsync(int studyPlanId, CancellationToken ct);
    Task<List<int>> GetSubjectIdsInPlanAsync(int studyPlanId, CancellationToken ct);
    Task<List<(int SubjectId, int PrerequisiteSubjectId)>> GetPairsByPlanAsync(int studyPlanId, CancellationToken ct);
    Task ReplaceAsync(int studyPlanId, int subjectId, IReadOnlyList<int> prerequisiteSubjectIds, CancellationToken ct);
}