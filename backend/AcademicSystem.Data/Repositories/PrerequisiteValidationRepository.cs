using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Repositories;

public sealed class PrerequisiteValidationRepository(AcademicSystemContext context)
    : IPrerequisiteValidationRepository
{
    public Task<int?> GetStudentIdByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        context.Students
            .AsNoTracking()
            .Where(student => student.UserId == userId)
            .Select(student => (int?)student.Id)
            .SingleOrDefaultAsync(cancellationToken);

    public Task<bool> StudyPlanExistsAsync(
        int studyPlanId,
        CancellationToken cancellationToken = default) =>
        context.StudyPlans
            .AsNoTracking()
            .AnyAsync(plan => plan.Id == studyPlanId, cancellationToken);

    public Task<bool> SubjectExistsAsync(
        int subjectId,
        CancellationToken cancellationToken = default) =>
        context.Subjects
            .AsNoTracking()
            .AnyAsync(subject => subject.Id == subjectId, cancellationToken);

    public Task<bool> SubjectBelongsToPlanAsync(
        int studyPlanId,
        int subjectId,
        CancellationToken cancellationToken = default) =>
        context.StudyPlanSubjects
            .AsNoTracking()
            .AnyAsync(
                item => item.StudyPlanId == studyPlanId &&
                    item.SubjectId == subjectId,
                cancellationToken);

    public async Task<IReadOnlyList<PrerequisiteSubjectDto>> GetPrerequisitesAsync(
        int studyPlanId,
        int subjectId,
        CancellationToken cancellationToken = default) =>
        await context.Prerequisites
            .AsNoTracking()
            .Where(item =>
                item.StudyPlanId == studyPlanId &&
                item.SubjectId == subjectId)
            .Select(item => new PrerequisiteSubjectDto
            {
                SubjectId = item.PrerequisiteSubjectId,
                Name = item.PrerequisiteSubject.Name
            })
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<int>> GetApprovedPrerequisiteSubjectIdsAsync(
        int studentId,
        IReadOnlyList<int> prerequisiteSubjectIds,
        CancellationToken cancellationToken = default) =>
        await context.StudentSubjects
            .AsNoTracking()
            .Where(item =>
                item.StudentId == studentId &&
                prerequisiteSubjectIds.Contains(item.SubjectId) &&
                item.CourseRecord != null &&
                item.CourseRecord.FinalStatus == "Aprobado")
            .Select(item => item.SubjectId)
            .ToListAsync(cancellationToken);
}
