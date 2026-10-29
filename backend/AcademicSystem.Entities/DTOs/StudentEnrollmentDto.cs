namespace AcademicSystem.Entities.DTOs;

public sealed record StudentEnrollmentDto(
    int EnrollmentId,
    int SubjectId,
    string SubjectName,
    string Section);
