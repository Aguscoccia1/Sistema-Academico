using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;

namespace AcademicSystem.Data.Repositories;

public interface IEnrollmentRepository
{
    Task<int?> GetStudentIdByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AvailableSubjectDto>> GetAvailableSubjectsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentEnrollmentDto>> GetStudentEnrollmentsAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    Task<StudentSubject?> GetEnrollmentAsync(
        int studentId,
        int subjectId,
        CancellationToken cancellationToken = default);

    Task<EnrollmentCreationResult> CreateEnrollmentAsync(
        int studentId,
        int subjectId,
        CancellationToken cancellationToken = default);
}

public enum EnrollmentCreationStatus
{
    Created,
    AlreadyExists,
    NoAvailableSections
}

public sealed record EnrollmentCreationResult(
    EnrollmentCreationStatus Status,
    StudentSubject? Enrollment = null,
    string? ExistingFinalStatus = null);
