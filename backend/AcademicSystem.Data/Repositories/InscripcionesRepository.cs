using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Repositories;

public sealed class InscripcionesRepository(AcademicSystemContext context)
    : IInscripcionesRepository
{
    public async Task<IReadOnlyList<EstudianteResponseDto>> GetEstudiantesAsync(
        CancellationToken cancellationToken = default) =>
        await (
            from student in context.Students
            join user in context.Users on student.UserId equals user.Id
            orderby student.Id
            select new EstudianteResponseDto(
                student.Id,
                student.FirstName,
                student.LastName,
                student.Dni,
                student.RecordNumber,
                user.Email))
        .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<MateriaResponseDto>> GetMateriasPorAnioAsync(
        int anio,
        CancellationToken cancellationToken = default)
    {
        var materias = await context.StudyPlanSubjects
            .AsNoTracking()
            .Where(studyPlanSubject => studyPlanSubject.CourseYear == anio)
            .Select(studyPlanSubject => new
            {
                studyPlanSubject.SubjectId,
                Nombre = studyPlanSubject.Subject.Name,
                studyPlanSubject.CourseYear
            })
            .Distinct()
            .OrderBy(subject => subject.Nombre)
            .ToListAsync(cancellationToken);

        return materias
            .Select(subject => new MateriaResponseDto(
                subject.SubjectId,
                subject.Nombre,
                subject.CourseYear))
            .ToList();
    }

    public Task<int> CountInscriptosAsync(
        CancellationToken cancellationToken = default) =>
        context.StudentSubjects
            .Select(enrollment => enrollment.StudentId)
            .Distinct()
            .CountAsync(cancellationToken);

    public async Task<IReadOnlyList<InscripcionResponseDto>> GetPorEstudianteAsync(
        int estudianteId,
        CancellationToken cancellationToken = default) =>
        await context.StudentSubjects
            .Where(enrollment => enrollment.StudentId == estudianteId)
            .OrderBy(enrollment => enrollment.Subject.Name)
            .Select(enrollment => new InscripcionResponseDto(
                enrollment.Id,
                enrollment.StudentId,
                enrollment.SubjectId,
                enrollment.Subject.Name,
                enrollment.Section))
            .ToListAsync(cancellationToken);

    public Task<bool> EstudianteExistsAsync(
        int estudianteId,
        CancellationToken cancellationToken = default) =>
        context.Students.AnyAsync(student => student.Id == estudianteId, cancellationToken);

    public Task<bool> MateriaExistsAsync(
        int materiaId,
        CancellationToken cancellationToken = default) =>
        context.Subjects.AnyAsync(subject => subject.Id == materiaId, cancellationToken);

    public Task<bool> InscripcionExistsAsync(
        int estudianteId,
        int materiaId,
        CancellationToken cancellationToken = default) =>
        context.StudentSubjects.AnyAsync(
            enrollment =>
                enrollment.StudentId == estudianteId &&
                enrollment.SubjectId == materiaId,
            cancellationToken);

    public void Add(StudentSubject inscripcion) =>
        context.StudentSubjects.Add(inscripcion);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
