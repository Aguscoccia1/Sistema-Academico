using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Data.Repositories;

public interface IComisionesRepository
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
