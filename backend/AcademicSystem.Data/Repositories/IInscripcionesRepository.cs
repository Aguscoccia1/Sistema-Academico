using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;

namespace AcademicSystem.Data.Repositories;

public interface IInscripcionesRepository
{
    Task<IReadOnlyList<EstudianteResponseDto>> GetEstudiantesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MateriaResponseDto>> GetMateriasPorAnioAsync(
        int anio,
        CancellationToken cancellationToken = default);

    Task<int> CountInscriptosAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InscripcionResponseDto>> GetPorEstudianteAsync(
        int estudianteId,
        CancellationToken cancellationToken = default);

    Task<bool> EstudianteExistsAsync(
        int estudianteId,
        CancellationToken cancellationToken = default);

    Task<bool> MateriaExistsAsync(
        int materiaId,
        CancellationToken cancellationToken = default);

    Task<bool> InscripcionExistsAsync(
        int estudianteId,
        int materiaId,
        CancellationToken cancellationToken = default);

    void Add(StudentSubject inscripcion);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
