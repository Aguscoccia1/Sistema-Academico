using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Data.Repositories;

public interface ITeacherSubjectRepository
{
    Task<TeacherSubjectsResponseDto?> GetAssignmentsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<UpdateTeacherSubjectsResultDto> ReplaceAssignmentsAsync(
        int userId,
        IReadOnlyList<UpdateTeacherSubjectDto> assignments,
        CancellationToken cancellationToken = default);
}
