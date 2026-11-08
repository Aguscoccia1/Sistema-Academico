using AcademicSystem.Entities.DTOs;
 
namespace AcademicSystem.Business.Services;
 
public interface IPrerequisiteService
{
    Task<List<PrerequisiteItemDto>?> GetByPlanAsync(int studyPlanId, CancellationToken ct);  
 
    Task<List<PrerequisiteItemDto>> ReplaceAsync(int studyPlanId, int subjectId, IReadOnlyList<int> prerequisiteSubjectIds, CancellationToken ct);
}