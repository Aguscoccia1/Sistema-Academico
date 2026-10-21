namespace AcademicSystem.Entities.DTOs;

/// <summary>Subject offered for enrollment in an active study plan.</summary>
public sealed record AvailableSubjectDto(
    int Id,
    int Code,
    string Name,
    int CourseYear);

/// <summary>Available subjects and an optional message when the list is empty.</summary>
public sealed record AvailableSubjectsResponseDto(
    string? Message,
    IReadOnlyList<AvailableSubjectDto> Subjects);
