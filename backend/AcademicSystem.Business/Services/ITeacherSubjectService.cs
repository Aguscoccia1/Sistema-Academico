using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public interface ITeacherSubjectService
{
    Task<List<TeacherResponseDto>> GetAllTeachersAsync(
        CancellationToken cancellationToken = default);

    Task<TeacherSubjectsResponseDto?> GetAssignmentsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<UpdateTeacherSubjectsResultDto> ReplaceAssignmentsAsync(
        int userId,
        IReadOnlyList<UpdateTeacherSubjectDto> assignments,
        CancellationToken cancellationToken = default);
}
