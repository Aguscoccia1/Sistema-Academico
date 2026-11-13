using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Business.Services;

public interface IInscripcionesService
{
    Task<IReadOnlyList<EstudianteResponseDto>> ObtenerEstudiantesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MateriaResponseDto>> ObtenerMateriasPorAnioAsync(
        int anio,
        CancellationToken cancellationToken = default);

    Task<int> ObtenerCantidadAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InscripcionResponseDto>> ObtenerPorEstudianteAsync(
        int estudianteId,
        CancellationToken cancellationToken = default);

    Task<ResultadoInscripcionDto> InscribirAsync(
        CrearInscripcionDto solicitud,
        CancellationToken cancellationToken = default);
}

public sealed class InscripcionesService(IInscripcionesRepository inscripcionesRepository, IComisionesRepository comisionesRepository) : IInscripcionesService
{
    public Task<IReadOnlyList<EstudianteResponseDto>> ObtenerEstudiantesAsync(
        CancellationToken cancellationToken = default)
        => inscripcionesRepository.GetEstudiantesAsync(cancellationToken);

    public Task<IReadOnlyList<MateriaResponseDto>> ObtenerMateriasPorAnioAsync(
        int anio,
        CancellationToken cancellationToken = default) =>
        inscripcionesRepository.GetMateriasPorAnioAsync(anio, cancellationToken);

    public Task<int> ObtenerCantidadAsync(
        CancellationToken cancellationToken = default) =>
        inscripcionesRepository.CountInscriptosAsync(cancellationToken);

    public Task<IReadOnlyList<InscripcionResponseDto>> ObtenerPorEstudianteAsync(
        int estudianteId,
        CancellationToken cancellationToken = default) =>
        inscripcionesRepository.GetPorEstudianteAsync(estudianteId, cancellationToken);

    public async Task<ResultadoInscripcionDto> InscribirAsync(
        CrearInscripcionDto solicitud,
        CancellationToken cancellationToken = default)
    {
        if (!await inscripcionesRepository.EstudianteExistsAsync(
                solicitud.EstudianteId, cancellationToken))
        {
            return new ResultadoInscripcionDto(false, "El estudiante no existe.");
        }

        if (!await inscripcionesRepository.MateriaExistsAsync(
                solicitud.MateriaId, cancellationToken))
        {
            return new ResultadoInscripcionDto(false, "La materia no existe.");
        }

        var comisionEstandar = solicitud.Seccion is "A" or "B";
        var comisionConfigurada = await comisionesRepository.SeccionConfiguradaAsync(
            solicitud.MateriaId, solicitud.Seccion, cancellationToken);

        if (!comisionEstandar && !comisionConfigurada)
        {
            return new ResultadoInscripcionDto(false, "La comisión no existe para esa materia.");
        }

        if (await inscripcionesRepository.InscripcionExistsAsync(
                solicitud.EstudianteId, solicitud.MateriaId, cancellationToken))
        {
            return new ResultadoInscripcionDto(
                false, "El estudiante ya está inscripto en esa materia.");
        }

        inscripcionesRepository.Add(new StudentSubject
        {
            StudentId = solicitud.EstudianteId,
            SubjectId = solicitud.MateriaId,
            Section = solicitud.Seccion
        });

        await inscripcionesRepository.SaveChangesAsync(cancellationToken);

        return new ResultadoInscripcionDto(true, "Inscripción realizada correctamente.");
    }
}
