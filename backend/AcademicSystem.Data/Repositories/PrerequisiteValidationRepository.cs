using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Repositories;

public sealed class PrerequisiteValidationRepository(AcademicSystemContext context) : IPrerequisiteValidationRepository
{
    public async Task<bool> StudyPlanExistsAsync(int studyPlanId, CancellationToken cancellationToken = default)
    {
        return await context.StudyPlans
            .AsNoTracking()
            .AnyAsync(plan => plan.Id == studyPlanId, cancellationToken);
    }

    public async Task<bool> SubjectExistsAsync(int subjectId, CancellationToken cancellationToken = default)
    {
        return await context.Subjects
            .AsNoTracking()
            .AnyAsync(subject => subject.Id == subjectId, cancellationToken);
    }

    public async Task<bool> SubjectBelongsToPlanAsync(int studyPlanId, int subjectId, CancellationToken cancellationToken = default)
    {
        return await context.StudyPlanSubjects
            .AsNoTracking()
            .AnyAsync(item => item.StudyPlanId == studyPlanId && item.SubjectId == subjectId, cancellationToken);
    }

    public async Task<IReadOnlyList<PrerequisiteSubjectDto>> GetPrerequisitesAsync(
        int studyPlanId,
        int subjectId,
        CancellationToken cancellationToken = default)
    {
        return await context.Prerequisites
            .AsNoTracking()
            .Where(item => item.StudyPlanId == studyPlanId && item.SubjectId == subjectId)
            .Select(item => new PrerequisiteSubjectDto
            {
                SubjectId = item.PrerequisiteSubjectId,
                Name = item.PrerequisiteSubject.Name
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetApprovedSubjectIdsAsync(
        int studentId,
        IReadOnlyList<int> subjectIds,
        string approvedStatus,
        CancellationToken cancellationToken = default)
    {
        if (subjectIds.Count == 0)
        {
            return Array.Empty<int>();
        }

        return await context.StudentSubjects
            .AsNoTracking()
            .Where(item => item.StudentId == studentId
                && subjectIds.Contains(item.SubjectId)
                && item.CourseRecord != null
                && item.CourseRecord.FinalStatus == approvedStatus)
            .Select(item => item.SubjectId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
