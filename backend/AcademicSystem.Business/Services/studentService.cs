
using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Business.Services;

public interface IStudentService
{
    Task<EstudianteResponseDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<EstadoModificacionEstudiante> ModificarAsync(
        int id,
        ModificarEstudianteDto datos,
        CancellationToken cancellationToken = default);

    Task<EstadoReasignacionComision> ReasignarComisionAsync(
        int estudianteId,
        int materiaId,
        string seccion,
        CancellationToken cancellationToken = default);
}

public class StudentService(AcademicSystemContext context) : IStudentService
{
    public async Task<EstudianteResponseDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await (
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
    }

    public async Task<EstadoModificacionEstudiante> ModificarAsync(
        int id,
        ModificarEstudianteDto datos,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await context.Students
            .FirstOrDefaultAsync(student => student.Id == id, cancellationToken);

        if (estudiante is null)
            return EstadoModificacionEstudiante.NoEncontrado;

        var dni = datos.Dni.Trim();
        if (await context.Students.AnyAsync(
                student => student.Dni == dni && student.Id != id,
                cancellationToken))
        {
            return EstadoModificacionEstudiante.DniDuplicado;
        }

        estudiante.FirstName = datos.Nombre.Trim();
        estudiante.LastName = datos.Apellido.Trim();
        estudiante.Dni = dni;

        await context.SaveChangesAsync(cancellationToken);
        return EstadoModificacionEstudiante.Actualizado;
    }

    public async Task<EstadoReasignacionComision> ReasignarComisionAsync(
        int estudianteId,
        int materiaId,
        string seccion,
        CancellationToken cancellationToken = default)
    {
        var inscripcion = await context.StudentSubjects
            .FirstOrDefaultAsync(
                enrollment =>
                    enrollment.StudentId == estudianteId &&
                    enrollment.SubjectId == materiaId,
                cancellationToken);

        if (inscripcion is null)
            return EstadoReasignacionComision.InscripcionNoEncontrada;

        var seccionNormalizada = seccion.Trim();
        var comisionEstandar = seccionNormalizada is "A" or "B";
        var comisionConfigurada =
            await context.TeacherSubjects.AnyAsync(
                assignment =>
                    assignment.SubjectId == materiaId &&
                    assignment.Section == seccionNormalizada,
                cancellationToken) ||
            await context.SubjectSchedules.AnyAsync(
                schedule =>
                    schedule.SubjectId == materiaId &&
                    schedule.Section == seccionNormalizada,
                cancellationToken) ||
            await context.StudentSubjects.AnyAsync(
                enrollment =>
                    enrollment.SubjectId == materiaId &&
                    enrollment.Section == seccionNormalizada,
                cancellationToken);

        if (!comisionEstandar && !comisionConfigurada)
        {
            return EstadoReasignacionComision.ComisionInexistente;
        }

        if (inscripcion.Section == seccionNormalizada)
            return EstadoReasignacionComision.YaAsignada;

        inscripcion.Section = seccionNormalizada;
        await context.SaveChangesAsync(cancellationToken);

        return EstadoReasignacionComision.Reasignada;
    }
}
