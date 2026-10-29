using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public sealed class TeacherSubjectService(ITeacherSubjectRepository repository)
    : ITeacherSubjectService
{
    public Task<TeacherSubjectsResponseDto?> GetAssignmentsAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        repository.GetAssignmentsAsync(userId, cancellationToken);

    public Task<UpdateTeacherSubjectsResultDto> ReplaceAssignmentsAsync(
        int userId,
        IReadOnlyList<UpdateTeacherSubjectDto> assignments,
        CancellationToken cancellationToken = default) =>
        repository.ReplaceAssignmentsAsync(userId, assignments, cancellationToken);
}
