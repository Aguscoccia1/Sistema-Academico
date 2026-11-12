using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Repositories;

public sealed class StudentRepository(AcademicSystemContext context) : IStudentRepository
{
    public Task<EstudianteResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        (
            from student in context.Students
            join user in context.Users on student.UserId equals user.Id
            where student.Id == id
            select new EstudianteResponseDto(
                student.Id,
                student.FirstName,
                student.LastName,
                student.Dni,
                student.RecordNumber,
                user.Email))
        .FirstOrDefaultAsync(cancellationToken);

    public Task<Student?> FindAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        context.Students.FirstOrDefaultAsync(student => student.Id == id, cancellationToken);

    public Task<bool> DniExistsForOtherAsync(
        int id,
        string dni,
        CancellationToken cancellationToken = default) =>
        context.Students.AnyAsync(
            student => student.Dni == dni && student.Id != id,
            cancellationToken);

    public Task<StudentSubject?> FindEnrollmentAsync(
        int estudianteId,
        int materiaId,
        CancellationToken cancellationToken = default) =>
        context.StudentSubjects.FirstOrDefaultAsync(
            enrollment =>
                enrollment.StudentId == estudianteId &&
                enrollment.SubjectId == materiaId,
            cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
