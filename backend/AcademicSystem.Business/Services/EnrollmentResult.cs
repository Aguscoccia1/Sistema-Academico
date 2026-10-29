using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public enum EnrollmentResultStatus
{
    Created,
    NotFound,
    Conflict,
    PrerequisitesNotMet,
    InvalidRequest
}

public sealed record EnrollmentResult(
    EnrollmentResultStatus Status,
    StudentSubjectResponseDto? Enrollment = null,
    string? Message = null,
    PrerequisiteValidationResponseDto? Validation = null);
