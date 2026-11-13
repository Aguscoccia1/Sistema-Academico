using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;
 
namespace AcademicSystem.Business.Services;
 
public class PrerequisiteService : IPrerequisiteService
{
    private readonly IPrerequisiteRepository _prerequisiteRepository;
    private readonly IStudyPlanRepository _studyPlanRepository;
 
    public PrerequisiteService(IPrerequisiteRepository prerequisiteRepository, IStudyPlanRepository studyPlanRepository)
    {
        _prerequisiteRepository = prerequisiteRepository;
        _studyPlanRepository = studyPlanRepository;
    }
 
    public async Task<List<PrerequisiteItemDto>?> GetByPlanAsync(int studyPlanId, CancellationToken ct)
    {
        if (!await _studyPlanRepository.ExistsAsync(studyPlanId, ct))
        {
            return null;
        }
 
        return await _prerequisiteRepository.GetByPlanAsync(studyPlanId, ct);
    }
 
    public async Task<List<PrerequisiteItemDto>> ReplaceAsync(int studyPlanId, int subjectId, IReadOnlyList<int> prerequisiteSubjectIds, CancellationToken ct)
    {
        if (!await _studyPlanRepository.ExistsAsync(studyPlanId, ct))
        {
            throw new KeyNotFoundException("No se encontró el plan de estudio.");
        }
 
        List<int> subjectIdsInPlan = await _prerequisiteRepository.GetSubjectIdsInPlanAsync(studyPlanId, ct);
 
        if (!subjectIdsInPlan.Contains(subjectId))
        {
            throw new KeyNotFoundException("La materia no pertenece a este plan de estudio.");
        }
 
        if (prerequisiteSubjectIds.Any(id => !subjectIdsInPlan.Contains(id)))
        {
            throw new ArgumentException("Todas las correlativas tienen que pertenecer al mismo plan de estudio.");
        }
 
        if (prerequisiteSubjectIds.Contains(subjectId))
        {
            throw new ArgumentException("Una materia no puede ser correlativa de sí misma.");
        }
 
        if (prerequisiteSubjectIds.Distinct().Count() != prerequisiteSubjectIds.Count)
        {
            throw new ArgumentException("Hay correlativas repetidas.");
        }
 
        List<(int SubjectId, int PrerequisiteSubjectId)> pairs = await _prerequisiteRepository.GetPairsByPlanAsync(studyPlanId, ct);
 
        if (WouldCreateCycle(subjectId, prerequisiteSubjectIds, pairs))
        {
            throw new ArgumentException("Las correlativas elegidas forman un ciclo: una materia no puede requerir, directa o indirectamente, a otra que depende de ella.");
        }
 
        await _prerequisiteRepository.ReplaceAsync(studyPlanId, subjectId, prerequisiteSubjectIds, ct);
 
        return await _prerequisiteRepository.GetByPlanAsync(studyPlanId, ct);
    }
 
    private static bool WouldCreateCycle(int subjectId, IReadOnlyList<int> newPrerequisites, List<(int SubjectId, int PrerequisiteSubjectId)> pairs)
    {
        Dictionary<int, List<int>> requirements = pairs
            .Where(pair => pair.SubjectId != subjectId)
            .GroupBy(pair => pair.SubjectId)
            .ToDictionary(group => group.Key, group => group.Select(pair => pair.PrerequisiteSubjectId).ToList());
 
        HashSet<int> visited = new HashSet<int>();
        Stack<int> pending = new Stack<int>(newPrerequisites);
 
        while (pending.Count > 0)
        {
            int current = pending.Pop();
 
            if (current == subjectId)
            {
                return true;
            }
 
            if (!visited.Add(current))
            {
                continue;
            }
 
            if (requirements.TryGetValue(current, out List<int>? next))
            {
                foreach (int prerequisiteId in next)
                {
                    pending.Push(prerequisiteId);
                }
            }
        }
 
        return false;
    }
}