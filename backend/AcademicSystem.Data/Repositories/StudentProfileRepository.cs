using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Repositories;

public class StudentProfileRepository(AcademicSystemContext context) : IStudentProfileRepository
{
    public Task<List<StudentProfileResponseDto>> GetAllAsync(CancellationToken ct) =>
        context.Students.AsNoTracking()
            .OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
            .Select(s => new StudentProfileResponseDto(s.Id, s.UserId, s.FirstName, s.LastName, s.Dni, s.RecordNumber))
            .ToListAsync(ct);

    public Task<StudentProfileResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        context.Students.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentProfileResponseDto(s.Id, s.UserId, s.FirstName, s.LastName, s.Dni, s.RecordNumber))
            .SingleOrDefaultAsync(ct);

    public Task<Student?> FindAsync(int id, CancellationToken ct) =>
        context.Students.SingleOrDefaultAsync(s => s.Id == id, ct);

    public Task<bool> UserOrDocumentExistsAsync(int userId, string dni, string recordNumber, CancellationToken ct) =>
        context.Students.AnyAsync(s => s.UserId == userId || s.Dni == dni || s.RecordNumber == recordNumber, ct);

    public Task<bool> DocumentExistsForOtherAsync(int id, string dni, string recordNumber, CancellationToken ct) =>
        context.Students.AnyAsync(s => s.Id != id && (s.Dni == dni || s.RecordNumber == recordNumber), ct);

    public Task<bool> HasAcademicHistoryAsync(int id, CancellationToken ct) =>
        context.StudentSubjects.AnyAsync(ss => ss.StudentId == id, ct);

    public void Add(Student student) => context.Students.Add(student);

    public void Remove(Student student) => context.Students.Remove(student);

    public Task SaveChangesAsync(CancellationToken ct) => context.SaveChangesAsync(ct);
}
