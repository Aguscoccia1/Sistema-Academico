namespace AcademicSystem.Entities.DTOs;

public sealed record TeacherSubjectAssignmentDto(
    int SubjectId,
    string SubjectName,
    string Section);

public sealed record TeacherSubjectsResponseDto(
    int TeacherId,
    string FirstName,
    string LastName,
    string Email,
    IReadOnlyList<TeacherSubjectAssignmentDto> Assignments);

public sealed record UpdateTeacherSubjectDto(
    int SubjectId,
    string Section);

public sealed record UpdateTeacherSubjectsRequestDto(
    IReadOnlyList<UpdateTeacherSubjectDto> Assignments);

public sealed record UpdateTeacherSubjectsResultDto(
    bool TeacherFound,
    bool Success,
    string Message);
