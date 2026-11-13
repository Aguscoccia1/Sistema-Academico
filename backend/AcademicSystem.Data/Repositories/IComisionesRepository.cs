using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;

namespace AcademicSystem.Data.Repositories;

public interface IComisionesRepository
{
    Task<bool> SeccionConfiguradaAsync(
        int materiaId,
        string seccion,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ComisionResponseDto>> ObtenerTodasAsync(
        int? materiaId,
        CancellationToken cancellationToken = default);

    Task<EstudianteResponseDto?> BuscarEstudianteAsync(
        int materiaId,
        string seccion,
        int estudianteId,
        CancellationToken cancellationToken = default);

    Task<T> EjecutarConBloqueoAsync<T>(
        int materiaId,
        Func<Task<T>> accion,
        CancellationToken cancellationToken = default);

    Task<T> EjecutarAsignacionMasivaConBloqueoAsync<T>(
        IReadOnlyCollection<int> materiaIds,
        Func<Task<T>> accion,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetMateriasDePrimerAnioAsync(
        IReadOnlyCollection<int> materiaIds,
        CancellationToken cancellationToken = default);

    Task<string?> GetNombreMateriaAsync(
        int materiaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetSeccionesConfiguradasAsync(
        int materiaId,
        CancellationToken cancellationToken = default);

    Task<List<StudentSubject>> GetInscripcionesAsync(
        int materiaId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
