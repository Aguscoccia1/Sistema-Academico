
using AcademicSystem.Data.Context;
using AcademicSystem.Entities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Business.Services;

public interface IComisionesService
{
    Task<IReadOnlyList<ComisionResponseDto>> ObtenerTodasAsync(
        int? materiaId,
        CancellationToken cancellationToken = default);

    Task<EstudianteResponseDto?> BuscarEstudianteAsync(
        int materiaId,
        string seccion,
        int estudianteId,
        CancellationToken cancellationToken = default);

    Task<AsignacionAleatoriaMasivaResponseDto?> AsignarAleatoriamenteAsync(
        int materiaId,
        CancellationToken cancellationToken = default);
}

public class ComisionesService(AcademicSystemContext context) : IComisionesService
{
    public async Task<IReadOnlyList<ComisionResponseDto>> ObtenerTodasAsync(
        int? materiaId,
        CancellationToken cancellationToken = default)
    {
        var comisionesDocentes = context.TeacherSubjects
            .Where(ts => materiaId == null || ts.SubjectId == materiaId)
            .Select(ts => new
            {
                ts.SubjectId,
                Materia = ts.Subject.Name,
                Seccion = ts.Section
            });

        var comisionesConInscriptos = context.StudentSubjects
            .Where(ss => materiaId == null || ss.SubjectId == materiaId)
            .Select(ss => new
            {
                ss.SubjectId,
                Materia = ss.Subject.Name,
                Seccion = ss.Section
            });

        return await comisionesDocentes
            .Union(comisionesConInscriptos)
            .Distinct()
            .Select(comision => new ComisionResponseDto(
                comision.SubjectId,
                comision.Materia,
                comision.Seccion,
                context.StudentSubjects.Count(ss =>
                    ss.SubjectId == comision.SubjectId &&
                    ss.Section == comision.Seccion)))
            .OrderBy(comision => comision.MateriaId)
            .ThenBy(comision => comision.Seccion)
            .ToListAsync(cancellationToken);
    }

    public async Task<EstudianteResponseDto?> BuscarEstudianteAsync(
        int materiaId,
        string seccion,
        int estudianteId,
        CancellationToken cancellationToken = default)
    {
        return await context.StudentSubjects
            .Where(ss =>
                ss.SubjectId == materiaId &&
                ss.Section == seccion &&
                ss.StudentId == estudianteId)
            .Select(ss => new EstudianteResponseDto(
                ss.Student.Id,
                ss.Student.FirstName,
                ss.Student.LastName,
                ss.Student.Dni,
                ss.Student.RecordNumber))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AsignacionAleatoriaMasivaResponseDto?> AsignarAleatoriamenteAsync(
        int materiaId,
        CancellationToken cancellationToken = default)
    {
        var materia = await context.Subjects
            .Where(subject => subject.Id == materiaId)
            .Select(subject => subject.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (materia is null)
            return null;

        var secciones = await context.TeacherSubjects
            .Where(assignment => assignment.SubjectId == materiaId)
            .Select(assignment => assignment.Section)
            .Distinct()
            .ToListAsync(cancellationToken);

        secciones = secciones
            .Where(section => !string.IsNullOrWhiteSpace(section))
            .ToList();

        if (secciones.Count == 0)
        {
            return new AsignacionAleatoriaMasivaResponseDto(
                false,
                "La materia no tiene comisiones configuradas.",
                0,
                []);
        }

        var inscripcionesMateria = await context.StudentSubjects
            .Where(enrollment => enrollment.SubjectId == materiaId)
            .ToListAsync(cancellationToken);

        var inscripcionesSinComision = inscripcionesMateria
            .Where(enrollment => string.IsNullOrWhiteSpace(enrollment.Section))
            .ToList();

        Shuffle(inscripcionesSinComision);
        Shuffle(secciones);

        var cantidadPorSeccion = secciones.ToDictionary(
            section => section,
            _ => 0);

        for (var index = 0; index < inscripcionesSinComision.Count; index++)
        {
            var seccion = secciones[index % secciones.Count];
            inscripcionesSinComision[index].Section = seccion;
            cantidadPorSeccion[seccion]++;
        }

        if (inscripcionesSinComision.Count > 0)
            await context.SaveChangesAsync(cancellationToken);

        return new AsignacionAleatoriaMasivaResponseDto(
            true,
            inscripcionesSinComision.Count == 0
                ? "No hay inscripciones pendientes de asignación."
                : $"Se asignó una comisión a {inscripcionesSinComision.Count} inscripciones.",
            inscripcionesSinComision.Count,
            cantidadPorSeccion
                .OrderBy(pair => pair.Key)
                .Select(pair => new DistribucionComisionDto(pair.Key, pair.Value))
                .ToList());
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
