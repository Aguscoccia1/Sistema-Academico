
using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;

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
        IReadOnlyCollection<int> materiaIds,
        CancellationToken cancellationToken = default);

}

public sealed class ComisionesService(IComisionesRepository repository) : IComisionesService
{
    public Task<IReadOnlyList<ComisionResponseDto>> ObtenerTodasAsync(
        int? materiaId,
        CancellationToken cancellationToken = default)
        => repository.ObtenerTodasAsync(materiaId, cancellationToken);

    public Task<EstudianteResponseDto?> BuscarEstudianteAsync(
        int materiaId,
        string seccion,
        int estudianteId,
        CancellationToken cancellationToken = default)
        => repository.BuscarEstudianteAsync(
            materiaId, seccion, estudianteId, cancellationToken);

    public Task<AsignacionAleatoriaMasivaResponseDto?> AsignarAleatoriamenteAsync(
        IReadOnlyCollection<int> materiaIds,
        CancellationToken cancellationToken = default)
        => repository.EjecutarAsignacionMasivaConBloqueoAsync<AsignacionAleatoriaMasivaResponseDto?>(
            () => AsignarAleatoriamenteDentroDelBloqueoAsync(
                materiaIds, cancellationToken),
            cancellationToken);

    private async Task<AsignacionAleatoriaMasivaResponseDto?>
        AsignarAleatoriamenteDentroDelBloqueoAsync(
            IReadOnlyCollection<int> materiaIds,
            CancellationToken cancellationToken)
    {
        var materias = new List<(int Id, string Nombre)>();
        foreach (var materiaId in materiaIds.Distinct().Order())
        {
            var nombre = await repository.GetNombreMateriaAsync(
                materiaId, cancellationToken);
            if (nombre is null)
                return null;

            var seccionesConfiguradas = await repository.GetSeccionesConfiguradasAsync(
                materiaId, cancellationToken);
            var secciones = seccionesConfiguradas
                .Select(NormalizarSeccion)
                .OfType<string>()
                .ToHashSet(StringComparer.Ordinal);
            if (!secciones.Contains("A") || !secciones.Contains("B"))
            {
                return new AsignacionAleatoriaMasivaResponseDto(
                    false,
                    $"La materia {nombre} necesita horarios configurados para las comisiones A y B. No se realizaron cambios.",
                    0,
                    []);
            }

            materias.Add((materiaId, nombre));
        }

        if (materias.Count == 0)
        {
            return new AsignacionAleatoriaMasivaResponseDto(
                false,
                "No se recibieron materias para asignar.",
                0,
                []);
        }

        var inscripcionesExistentes = await repository.GetAllEnrollmentsAsync(
            cancellationToken);
        var seccionesPorEstudiante = inscripcionesExistentes
            .Select(enrollment => new
            {
                enrollment.StudentId,
                Seccion = NormalizarSeccion(enrollment.Section)
            })
            .Where(enrollment => enrollment.Seccion is not null)
            .GroupBy(enrollment => enrollment.StudentId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(enrollment => enrollment.Seccion!)
                    .ToHashSet(StringComparer.Ordinal));

        var estudiantesConConflicto = seccionesPorEstudiante
            .Where(pair => pair.Value.Count > 1)
            .Select(pair => pair.Key)
            .ToHashSet();
        var cantidadPorSeccion = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["A"] = 0,
            ["B"] = 0
        };
        var seccionPorEstudiante = new Dictionary<int, string>();

        foreach (var (studentId, secciones) in seccionesPorEstudiante)
        {
            if (estudiantesConConflicto.Contains(studentId))
                continue;

            var seccion = secciones.Single();
            seccionPorEstudiante[studentId] = seccion;
            cantidadPorSeccion[seccion]++;
        }

        var estudiantesSinInscripcionPorMateria = new Dictionary<int, List<int>>();
        var estudiantesNuevosIds = new HashSet<int>();
        foreach (var materia in materias)
        {
            var studentIds = await repository.GetStudentIdsWithoutEnrollmentAsync(
                materia.Id, cancellationToken);
            estudiantesSinInscripcionPorMateria[materia.Id] = studentIds;
            foreach (var studentId in studentIds)
            {
                if (!seccionesPorEstudiante.ContainsKey(studentId) &&
                    !estudiantesConConflicto.Contains(studentId))
                {
                    estudiantesNuevosIds.Add(studentId);
                }
            }
        }

        var estudiantesNuevos = estudiantesNuevosIds.ToList();
        Shuffle(estudiantesNuevos);
        foreach (var studentId in estudiantesNuevos)
        {
            var seccion = cantidadPorSeccion["A"] == cantidadPorSeccion["B"]
                ? (Random.Shared.Next(2) == 0 ? "A" : "B")
                : cantidadPorSeccion["A"] < cantidadPorSeccion["B"] ? "A" : "B";
            seccionPorEstudiante[studentId] = seccion;
            cantidadPorSeccion[seccion]++;
        }

        if (Math.Abs(cantidadPorSeccion["A"] - cantidadPorSeccion["B"]) > 1)
        {
            return new AsignacionAleatoriaMasivaResponseDto(
                false,
                $"No se puede lograr una distribución equitativa sin mover inscripciones existentes (A: {cantidadPorSeccion["A"]}, B: {cantidadPorSeccion["B"]}). No se realizaron cambios.",
                0,
                []);
        }

        List<StudentSubject> nuevasInscripciones = [];
        foreach (var materia in materias)
        {
            foreach (var studentId in estudiantesSinInscripcionPorMateria[materia.Id])
            {
                if (estudiantesConConflicto.Contains(studentId))
                    continue;

                nuevasInscripciones.Add(new StudentSubject
                {
                    StudentId = studentId,
                    SubjectId = materia.Id,
                    Section = seccionPorEstudiante[studentId]
                });
            }
        }

        if (nuevasInscripciones.Count > 0)
        {
            repository.AddInscripciones(nuevasInscripciones);
            await repository.SaveChangesAsync(cancellationToken);
        }

        var distribucionFinal = $"Comisión A: {cantidadPorSeccion["A"]}; Comisión B: {cantidadPorSeccion["B"]}";
        var mensaje = nuevasInscripciones.Count == 0
            ? $"No había inscripciones nuevas para agregar. {distribucionFinal}."
            : $"Se agregaron {nuevasInscripciones.Count} inscripciones en {materias.Count} materias. {distribucionFinal}.";
        if (estudiantesConConflicto.Count > 0)
        {
            mensaje += $" Se omitieron {estudiantesConConflicto.Count} estudiantes que ya tenían inscripciones en ambas comisiones.";
        }

        return new AsignacionAleatoriaMasivaResponseDto(
            true,
            mensaje,
            nuevasInscripciones.Count,
            cantidadPorSeccion
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
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
