
using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;

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
        int materiaId,
        CancellationToken cancellationToken = default)
        => repository.AsignarAleatoriamenteAsync(materiaId, cancellationToken);

}
