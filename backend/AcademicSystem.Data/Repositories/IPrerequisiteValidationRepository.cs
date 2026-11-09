using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Data.Repositories;

public interface IPrerequisiteValidationRepository
{
    Task<bool> StudyPlanExistsAsync(int studyPlanId, CancellationToken cancellationToken = default);
    Task<bool> SubjectExistsAsync(int subjectId, CancellationToken cancellationToken = default);
    Task<bool> SubjectBelongsToPlanAsync(int studyPlanId, int subjectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PrerequisiteSubjectDto>> GetPrerequisitesAsync(int studyPlanId, int subjectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetApprovedSubjectIdsAsync(int studentId, IReadOnlyList<int> subjectIds, string approvedStatus, CancellationToken cancellationToken = default);
}
