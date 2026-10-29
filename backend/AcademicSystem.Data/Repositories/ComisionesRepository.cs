using System.Data;
using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Repositories;

public sealed class ComisionesRepository(AcademicSystemContext context) : IComisionesRepository
{
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

    public async Task<AsignacionAleatoriaMasivaResponseDto?> AsignarAleatoriamenteAsync(
        int materiaId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var materia = await context.Subjects
            .Where(subject => subject.Id == materiaId)
            .Select(subject => subject.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (materia is null)
            return null;

        var lockResource = $"comisiones-asignacion-materia-{materiaId}";
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

        var seccionesConfiguradas = await context.TeacherSubjects
            .AsNoTracking()
            .Where(assignment => assignment.SubjectId == materiaId)
            .Select(assignment => assignment.Section)
            .Concat(context.SubjectSchedules
                .AsNoTracking()
                .Where(schedule => schedule.SubjectId == materiaId)
                .Select(schedule => schedule.Section))
            .ToListAsync(cancellationToken);

        var inscripciones = await context.StudentSubjects
            .Where(enrollment => enrollment.SubjectId == materiaId)
            .ToListAsync(cancellationToken);

        var secciones = seccionesConfiguradas
            .Concat(inscripciones.Select(enrollment => enrollment.Section))
            .Select(NormalizarSeccion)
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .OrderBy(section => section)
            .ToList();

        if (secciones.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return new AsignacionAleatoriaMasivaResponseDto(
                false,
                "La materia no tiene comisiones A o B disponibles.",
                0,
                []);
        }

        var cantidadPorSeccion = secciones.ToDictionary(
            section => section,
            _ => 0,
            StringComparer.Ordinal);
        foreach (var inscripcion in inscripciones)
        {
            var seccion = NormalizarSeccion(inscripcion.Section);
            if (seccion is not null && cantidadPorSeccion.ContainsKey(seccion))
                cantidadPorSeccion[seccion]++;
        }

        var pendientes = inscripciones
            .Where(enrollment => string.IsNullOrWhiteSpace(enrollment.Section))
            .ToList();
        Shuffle(pendientes);

        var cantidadAsignadaPorSeccion = secciones.ToDictionary(
            section => section,
            _ => 0,
            StringComparer.Ordinal);

        foreach (var inscripcion in pendientes)
        {
            var menorCantidad = cantidadPorSeccion.Values.Min();
            var seccionesMenosPobladas = secciones
                .Where(section => cantidadPorSeccion[section] == menorCantidad)
                .ToArray();
            var seccion = seccionesMenosPobladas[
                Random.Shared.Next(seccionesMenosPobladas.Length)];

            inscripcion.Section = seccion;
            cantidadPorSeccion[seccion]++;
            cantidadAsignadaPorSeccion[seccion]++;
        }

        if (pendientes.Count > 0)
            await context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        var distribucionFinal = string.Join(
            "; ",
            secciones.Select(section => $"Comisión {section}: {cantidadPorSeccion[section]}"));
        var mensaje = pendientes.Count == 0
            ? $"No hay inscripciones pendientes. Distribución final: {distribucionFinal}."
            : $"Se asignaron {pendientes.Count} inscripciones. Distribución final: {distribucionFinal}.";

        return new AsignacionAleatoriaMasivaResponseDto(
            true,
            mensaje,
            pendientes.Count,
            cantidadAsignadaPorSeccion
                .OrderBy(pair => pair.Key)
                .Select(pair => new DistribucionComisionDto(pair.Key, pair.Value))
                .ToList());
    }

    private static string? NormalizarSeccion(string? seccion)
    {
        var seccionNormalizada = seccion?.Trim().ToUpperInvariant();
        return seccionNormalizada is "A" or "B" ? seccionNormalizada : null;
    }

    private static void Shuffle<T>(IList<T> items)
    {
        for (var index = items.Count - 1; index > 0; index--)
        {
            var randomIndex = Random.Shared.Next(index + 1);
            (items[index], items[randomIndex]) = (items[randomIndex], items[index]);
        }
    }
}
