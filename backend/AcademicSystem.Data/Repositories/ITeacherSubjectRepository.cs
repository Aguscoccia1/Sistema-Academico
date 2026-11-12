using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Data.Repositories;

public interface ITeacherSubjectRepository
{
    Task<List<TeacherResponseDto>> GetAllTeachersAsync(
        CancellationToken cancellationToken = default);

    Task<TeacherSubjectsResponseDto?> GetAssignmentsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetExistingSubjectIdsAsync(
        IReadOnlyList<int> subjectIds,
        CancellationToken cancellationToken = default);

    Task<bool> ReplaceAssignmentsAsync(
        int userId,
        IReadOnlyList<UpdateTeacherSubjectDto> assignments,
        CancellationToken cancellationToken = default);
}
