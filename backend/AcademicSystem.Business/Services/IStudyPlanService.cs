using AcademicSystem.Entities.DTOs;
 
namespace AcademicSystem.Business.Services;
 
public interface IStudyPlanService
{
    Task<List<StudyPlanResponseDto>> GetAllAsync(CancellationToken ct);
    Task<StudyPlanDetailResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<StudyPlanResponseDto> CreateAsync(StudyPlanRequestDto request, CancellationToken ct);
    Task<StudyPlanResponseDto?> UpdateAsync(int id, StudyPlanRequestDto request, CancellationToken ct);   // null si el plan no existe
    Task<StudyPlanSubjectResponseDto> AddSubjectAsync(int studyPlanId, StudyPlanSubjectRequestDto request, CancellationToken ct);
 
    Task<bool> PlanExistsAsync(int id, CancellationToken ct);
    Task<bool> ExistsByPlanCodeAsync(string planCode, int? excludeId, CancellationToken ct);
    Task<bool> SubjectExistsAsync(int subjectId, CancellationToken ct);
    Task<bool> SubjectInPlanAsync(int studyPlanId, int subjectId, CancellationToken ct);
}