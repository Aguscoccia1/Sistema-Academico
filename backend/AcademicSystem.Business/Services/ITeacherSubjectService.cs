using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public interface ITeacherSubjectService
{
    Task<TeacherSubjectsResponseDto?> GetAssignmentsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<UpdateTeacherSubjectsResultDto> ReplaceAssignmentsAsync(
        int userId,
        IReadOnlyList<UpdateTeacherSubjectDto> assignments,
        CancellationToken cancellationToken = default);
}
