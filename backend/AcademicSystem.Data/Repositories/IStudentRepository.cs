using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;

namespace AcademicSystem.Data.Repositories;

public interface IStudentRepository
{
    Task<EstudianteResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Student?> FindAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> DniExistsForOtherAsync(
        int id,
        string dni,
        CancellationToken cancellationToken = default);

    Task<StudentSubject?> FindEnrollmentAsync(
        int estudianteId,
        int materiaId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
