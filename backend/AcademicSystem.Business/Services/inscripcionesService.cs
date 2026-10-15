
using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Business.Services;

public interface IInscripcionesService
{
    Task<IReadOnlyList<EstudianteResponseDto>> ObtenerEstudiantesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MateriaResponseDto>> ObtenerMateriasPorAnioAsync(
        int anio,
        CancellationToken cancellationToken = default);

    Task<int> ObtenerCantidadAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InscripcionResponseDto>> ObtenerPorEstudianteAsync(
        int estudianteId,
        CancellationToken cancellationToken = default);

    Task<ResultadoInscripcionDto> InscribirAsync(
        CrearInscripcionDto solicitud,
        CancellationToken cancellationToken = default);
}

public class InscripcionesService(AcademicSystemContext context) : IInscripcionesService
{
    public async Task<IReadOnlyList<EstudianteResponseDto>> ObtenerEstudiantesAsync(
        CancellationToken cancellationToken = default)
    {
        return await context.Students
            .OrderBy(student => student.Id)
            .Select(student => new EstudianteResponseDto(
                student.Id,
                student.FirstName,
                student.LastName,
                student.Dni,
                student.RecordNumber))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MateriaResponseDto>> ObtenerMateriasPorAnioAsync(
        int anio,
        CancellationToken cancellationToken = default)
    {
        return await context.StudyPlanSubjects
            .Where(studyPlanSubject => studyPlanSubject.CourseYear == anio)
            .Select(studyPlanSubject => new MateriaResponseDto(
                studyPlanSubject.SubjectId,
                studyPlanSubject.Subject.Name,
                studyPlanSubject.CourseYear))
            .Distinct()
            .OrderBy(subject => subject.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ObtenerCantidadAsync(
        CancellationToken cancellationToken = default)
    {
        return await context.StudentSubjects
            .Select(enrollment => enrollment.StudentId)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InscripcionResponseDto>> ObtenerPorEstudianteAsync(
        int estudianteId,
        CancellationToken cancellationToken = default)
    {
        return await context.StudentSubjects
            .Where(enrollment => enrollment.StudentId == estudianteId)
            .OrderBy(enrollment => enrollment.Subject.Name)
            .Select(enrollment => new InscripcionResponseDto(
                enrollment.Id,
                enrollment.StudentId,
                enrollment.SubjectId,
                enrollment.Subject.Name,
                enrollment.Section))
            .ToListAsync(cancellationToken);
    }

    public async Task<ResultadoInscripcionDto> InscribirAsync(
        CrearInscripcionDto solicitud,
        CancellationToken cancellationToken = default)
    {
        if (!await context.Students.AnyAsync(
                student => student.Id == solicitud.EstudianteId,
                cancellationToken))
        {
            return new ResultadoInscripcionDto(false, "El estudiante no existe.");
        }

        if (!await context.Subjects.AnyAsync(
                subject => subject.Id == solicitud.MateriaId,
                cancellationToken))
        {
            return new ResultadoInscripcionDto(false, "La materia no existe.");
        }

        if (!await context.TeacherSubjects.AnyAsync(
                assignment =>
                    assignment.SubjectId == solicitud.MateriaId &&
                    assignment.Section == solicitud.Seccion,
                cancellationToken))
        {
            return new ResultadoInscripcionDto(false, "La comisión no existe para esa materia.");
        }

        if (await context.StudentSubjects.AnyAsync(
                enrollment =>
                    enrollment.StudentId == solicitud.EstudianteId &&
                    enrollment.SubjectId == solicitud.MateriaId,
                cancellationToken))
        {
            return new ResultadoInscripcionDto(
                false, "El estudiante ya está inscripto en esa materia.");
        }

        context.StudentSubjects.Add(new StudentSubject
        {
            StudentId = solicitud.EstudianteId,
            SubjectId = solicitud.MateriaId,
            Section = solicitud.Seccion
        });

        await context.SaveChangesAsync(cancellationToken);

        return new ResultadoInscripcionDto(true, "Inscripción realizada correctamente.");
    }
}
