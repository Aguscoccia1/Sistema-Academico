using System.Data;
using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Repositories;

public sealed class ComisionesRepository(AcademicSystemContext context) : IComisionesRepository
{
    public async Task<bool> SeccionConfiguradaAsync(
        int materiaId,
        string seccion,
        CancellationToken cancellationToken = default) =>
        await context.TeacherSubjects.AnyAsync(
            assignment =>
                assignment.SubjectId == materiaId &&
                assignment.Section == seccion,
            cancellationToken) ||
        await context.SubjectSchedules.AnyAsync(
            schedule =>
                schedule.SubjectId == materiaId &&
                schedule.Section == seccion,
            cancellationToken) ||
        await context.StudentSubjects.AnyAsync(
            enrollment =>
                enrollment.SubjectId == materiaId &&
                enrollment.Section == seccion,
            cancellationToken);

    public async Task<IReadOnlyList<ComisionResponseDto>> ObtenerTodasAsync(
        int? materiaId,
        CancellationToken cancellationToken = default)
    {
        var comisionesDocentes = await context.TeacherSubjects
            .AsNoTracking()
            .Where(assignment =>
                materiaId == null || assignment.SubjectId == materiaId)
            .Select(assignment => new
            {
                assignment.SubjectId,
                Materia = assignment.Subject.Name,
                Seccion = assignment.Section
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var comisionesConInscriptos = await context.StudentSubjects
            .AsNoTracking()
            .Where(enrollment =>
                materiaId == null || enrollment.SubjectId == materiaId)
            .Select(enrollment => new
            {
                enrollment.SubjectId,
                Materia = enrollment.Subject.Name,
                Seccion = enrollment.Section
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var comisionesConHorarios = await context.SubjectSchedules
            .AsNoTracking()
            .Where(schedule =>
                materiaId == null || schedule.SubjectId == materiaId)
            .Select(schedule => new
            {
                schedule.SubjectId,
                Materia = schedule.Subject.Name,
                Seccion = schedule.Section
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var cantidadPorComision = await context.StudentSubjects
            .AsNoTracking()
            .Where(enrollment =>
                materiaId == null || enrollment.SubjectId == materiaId)
            .GroupBy(enrollment => new
            {
                enrollment.SubjectId,
                enrollment.Section
            })
            .Select(group => new
            {
                group.Key.SubjectId,
                group.Key.Section,
                Cantidad = group.Count()
            })
            .ToListAsync(cancellationToken);

        var cantidades = cantidadPorComision.ToDictionary(
            item => (item.SubjectId, item.Section),
            item => item.Cantidad);

        return comisionesDocentes
            .Concat(comisionesConInscriptos)
            .Concat(comisionesConHorarios)
            .Distinct()
            .Select(comision => new ComisionResponseDto(
                comision.SubjectId,
                comision.Materia,
                comision.Seccion,
                cantidades.GetValueOrDefault((comision.SubjectId, comision.Seccion))))
            .OrderBy(comision => comision.MateriaId)
            .ThenBy(comision => comision.Seccion)
            .ToList();
    }

    public async Task<EstudianteResponseDto?> BuscarEstudianteAsync(
        int materiaId,
        string seccion,
        int estudianteId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from enrollment in context.StudentSubjects.AsNoTracking()
            join user in context.Users.AsNoTracking()
                on enrollment.Student.UserId equals user.Id
            where enrollment.SubjectId == materiaId &&
                enrollment.Section == seccion &&
                enrollment.StudentId == estudianteId
            select new EstudianteResponseDto(
                enrollment.Student.Id,
                enrollment.Student.FirstName,
                enrollment.Student.LastName,
                enrollment.Student.Dni,
                enrollment.Student.RecordNumber,
                user.Email))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<T> EjecutarConBloqueoAsync<T>(
        int materiaId,
        Func<Task<T>> accion,
        CancellationToken cancellationToken = default) =>
        await EjecutarConBloqueoAsync(
            [$"comisiones-asignacion-materia-{materiaId}"],
            accion,
            cancellationToken);

    public Task<T> EjecutarAsignacionMasivaConBloqueoAsync<T>(
        IReadOnlyCollection<int> materiaIds,
        Func<Task<T>> accion,
        CancellationToken cancellationToken = default) =>
        EjecutarConBloqueoAsync(
            materiaIds
                .Distinct()
                .Order()
                .Select(materiaId => $"comisiones-asignacion-materia-{materiaId}")
                .ToArray(),
            accion,
            cancellationToken);

    private async Task<T> EjecutarConBloqueoAsync<T>(
        IReadOnlyCollection<string> lockResources,
        Func<Task<T>> accion,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        foreach (var lockResource in lockResources)
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                DECLARE @lockResult int;
                EXEC @lockResult = sys.sp_getapplock
                    @Resource = {lockResource},
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Transaction',
                    @LockTimeout = -1;
                IF @lockResult < 0
                BEGIN
                    ;THROW 51001, 'No se pudo obtener el bloqueo para asignar comisiones.', 1;
                END;
                """,
                cancellationToken);
        }

        var resultado = await accion();
        await transaction.CommitAsync(cancellationToken);
        return resultado;
    }

    public Task<string?> GetNombreMateriaAsync(
        int materiaId,
        CancellationToken cancellationToken = default) =>
        context.Subjects
            .Where(subject => subject.Id == materiaId)
            .Select(subject => subject.Name)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<int>> GetMateriasDePrimerAnioAsync(
        IReadOnlyCollection<int> materiaIds,
        CancellationToken cancellationToken = default)
    {
        var materiasEnPlan = context.StudyPlanSubjects
            .AsNoTracking()
            .Where(planSubject =>
                planSubject.CourseYear == 1 &&
                materiaIds.Contains(planSubject.SubjectId))
            .Select(planSubject => planSubject.SubjectId);

        var materiasMarcadasComoPrimerAnio = context.Subjects
            .AsNoTracking()
            .Where(subject =>
                subject.Course == "Primer Año" &&
                materiaIds.Contains(subject.Id) &&
                !context.StudyPlanSubjects.Any(
                    planSubject => planSubject.SubjectId == subject.Id))
            .Select(subject => subject.Id);

        return await materiasEnPlan
            .Concat(materiasMarcadasComoPrimerAnio)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetSeccionesConfiguradasAsync(
        int materiaId,
        CancellationToken cancellationToken = default) =>
        await context.SubjectSchedules
            .AsNoTracking()
            .Where(schedule => schedule.SubjectId == materiaId)
            .Select(schedule => schedule.Section)
            .ToListAsync(cancellationToken);

    public Task<List<StudentSubject>> GetInscripcionesAsync(
        int materiaId,
        CancellationToken cancellationToken = default) =>
        context.StudentSubjects
            .Where(enrollment => enrollment.SubjectId == materiaId)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
