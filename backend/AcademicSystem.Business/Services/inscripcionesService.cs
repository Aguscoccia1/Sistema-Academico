
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
        return await (
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
    }

    public async Task<IReadOnlyList<MateriaResponseDto>> ObtenerMateriasPorAnioAsync(
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

        var comisionEstandar = solicitud.Seccion is "A" or "B";
        var comisionConfigurada =
            await context.TeacherSubjects.AnyAsync(
                assignment =>
                    assignment.SubjectId == solicitud.MateriaId &&
                    assignment.Section == solicitud.Seccion,
                cancellationToken) ||
            await context.SubjectSchedules.AnyAsync(
                schedule =>
                    schedule.SubjectId == solicitud.MateriaId &&
                    schedule.Section == solicitud.Seccion,
                cancellationToken) ||
            await context.StudentSubjects.AnyAsync(
                enrollment =>
                    enrollment.SubjectId == solicitud.MateriaId &&
                    enrollment.Section == solicitud.Seccion,
                cancellationToken);

        if (!comisionEstandar && !comisionConfigurada)
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
