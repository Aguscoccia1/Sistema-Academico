using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;

namespace AcademicSystem.Data.Repositories;

public interface IStudentProfileRepository
{
    Task<List<StudentProfileResponseDto>> GetAllAsync(CancellationToken ct);

    Task<StudentProfileResponseDto?> GetByIdAsync(int id, CancellationToken ct);

    Task<Student?> FindAsync(int id, CancellationToken ct);

    Task<bool> UserOrDocumentExistsAsync(int userId, string dni, string recordNumber, CancellationToken ct);

    Task<bool> DocumentExistsForOtherAsync(int id, string dni, string recordNumber, CancellationToken ct);

    Task<bool> HasAcademicHistoryAsync(int id, CancellationToken ct);

    void Add(Student student);

    void Remove(Student student);

    Task SaveChangesAsync(CancellationToken ct);
}
