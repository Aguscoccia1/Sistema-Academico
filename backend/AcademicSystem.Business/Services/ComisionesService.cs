
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
            materiaIds,
            () => AsignarAleatoriamenteDentroDelBloqueoAsync(
                materiaIds, cancellationToken),
            cancellationToken);

    private async Task<AsignacionAleatoriaMasivaResponseDto?>
        AsignarAleatoriamenteDentroDelBloqueoAsync(
            IReadOnlyCollection<int> materiaIds,
            CancellationToken cancellationToken)
    {
        var idsUnicos = materiaIds.Distinct().Order().ToList();
        if (idsUnicos.Count == 0)
        {
            return new AsignacionAleatoriaMasivaResponseDto(
                false,
                "No se recibieron materias para asignar.",
                0,
                []);
        }

        var materias = new List<(int Id, string Nombre)>();
        foreach (var materiaId in idsUnicos)
        {
            var nombre = await repository.GetNombreMateriaAsync(
                materiaId, cancellationToken);
            if (nombre is null)
                return null;

            materias.Add((materiaId, nombre));
        }

        var materiasPrimerAnio = (await repository.GetMateriasDePrimerAnioAsync(
            idsUnicos, cancellationToken)).ToHashSet();
        if (idsUnicos.Any(materiaId => !materiasPrimerAnio.Contains(materiaId)))
        {
            return new AsignacionAleatoriaMasivaResponseDto(
                false,
                "La asignación automática solo está disponible para materias de primer año. No se realizaron cambios.",
                0,
                []);
        }

        foreach (var materia in materias)
        {
            var seccionesConfiguradas = await repository.GetSeccionesConfiguradasAsync(
                materia.Id, cancellationToken);
            var secciones = seccionesConfiguradas
                .Select(NormalizarSeccion)
                .OfType<string>()
                .ToHashSet(StringComparer.Ordinal);
            if (!secciones.Contains("A") || !secciones.Contains("B"))
            {
                return new AsignacionAleatoriaMasivaResponseDto(
                    false,
                    $"La materia {materia.Nombre} necesita horarios configurados para las comisiones A y B. No se realizaron cambios.",
                    0,
                    []);
            }
        }

        var cantidadFinalPorSeccion = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["A"] = 0,
            ["B"] = 0
        };
        var totalReasignado = 0;
        var materiasConInscriptos = 0;

        foreach (var materia in materias)
        {
            var inscripciones = await repository.GetInscripcionesAsync(
                materia.Id, cancellationToken);
            if (inscripciones.Count == 0)
                continue;

            materiasConInscriptos++;
            var cantidadActualA = inscripciones.Count(enrollment =>
                NormalizarSeccion(enrollment.Section) == "A");
            var cantidadActualB = inscripciones.Count(enrollment =>
                NormalizarSeccion(enrollment.Section) == "B");
            var todasTienenComision = inscripciones.All(enrollment =>
                NormalizarSeccion(enrollment.Section) is "A" or "B");

            if (!todasTienenComision ||
                Math.Abs(cantidadActualA - cantidadActualB) > 1)
            {
                Shuffle(inscripciones);
                var cantidadA = inscripciones.Count / 2;
                if (inscripciones.Count % 2 == 1 && Random.Shared.Next(2) == 0)
                    cantidadA++;

                for (var index = 0; index < inscripciones.Count; index++)
                {
                    var seccion = index < cantidadA ? "A" : "B";
                    if (!string.Equals(
                            inscripciones[index].Section,
                            seccion,
                            StringComparison.Ordinal))
                    {
                        totalReasignado++;
                        inscripciones[index].Section = seccion;
                    }
                }

                cantidadActualA = cantidadA;
                cantidadActualB = inscripciones.Count - cantidadA;
            }

            cantidadFinalPorSeccion["A"] += cantidadActualA;
            cantidadFinalPorSeccion["B"] += cantidadActualB;
        }

        if (totalReasignado > 0)
            await repository.SaveChangesAsync(cancellationToken);

        var distribucionFinal =
            $"A: {cantidadFinalPorSeccion["A"]}; B: {cantidadFinalPorSeccion["B"]}";
        var mensaje = materiasConInscriptos == 0
            ? "No hay inscripciones existentes en las materias de primer año seleccionadas. No se crearon inscripciones."
            : totalReasignado == 0
                ? $"Las inscripciones existentes ya están distribuidas de forma equilibrada. {distribucionFinal}. No se crearon inscripciones."
                : $"Se redistribuyeron {totalReasignado} inscripciones existentes en {materiasConInscriptos} materias de primer año. {distribucionFinal}. No se crearon inscripciones.";

        return new AsignacionAleatoriaMasivaResponseDto(
            true,
            mensaje,
            totalReasignado,
            cantidadFinalPorSeccion
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
