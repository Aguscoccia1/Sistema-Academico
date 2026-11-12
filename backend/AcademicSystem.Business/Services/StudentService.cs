
using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

public interface IStudentService
{
    Task<EstudianteResponseDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<EstadoModificacionEstudiante> ModificarAsync(
        int id,
        ModificarEstudianteDto datos,
        CancellationToken cancellationToken = default);

    Task<EstadoReasignacionComision> ReasignarComisionAsync(
        int estudianteId,
        int materiaId,
        string seccion,
        CancellationToken cancellationToken = default);
}

public sealed class StudentService(
    IStudentRepository studentRepository,
    IComisionesRepository comisionesRepository) : IStudentService
{
    public Task<EstudianteResponseDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        studentRepository.GetByIdAsync(id, cancellationToken);

    public async Task<EstadoModificacionEstudiante> ModificarAsync(
        int id,
        ModificarEstudianteDto datos,
        CancellationToken cancellationToken = default)
    {
        var estudiante = await studentRepository.FindAsync(id, cancellationToken);

        if (estudiante is null)
            return EstadoModificacionEstudiante.NoEncontrado;

        var dni = datos.Dni.Trim();
        if (await studentRepository.DniExistsForOtherAsync(
                id, dni, cancellationToken))
        {
            return EstadoModificacionEstudiante.DniDuplicado;
        }

        estudiante.FirstName = datos.Nombre.Trim();
        estudiante.LastName = datos.Apellido.Trim();
        estudiante.Dni = dni;

        await studentRepository.SaveChangesAsync(cancellationToken);
        return EstadoModificacionEstudiante.Actualizado;
    }

    public async Task<EstadoReasignacionComision> ReasignarComisionAsync(
        int estudianteId,
        int materiaId,
        string seccion,
        CancellationToken cancellationToken = default)
    {
        var inscripcion = await studentRepository.FindEnrollmentAsync(
            estudianteId, materiaId, cancellationToken);

        if (inscripcion is null)
            return EstadoReasignacionComision.InscripcionNoEncontrada;

        var seccionNormalizada = seccion.Trim();
        var comisionEstandar = seccionNormalizada is "A" or "B";
        var comisionConfigurada = await comisionesRepository.SeccionConfiguradaAsync(
            materiaId, seccionNormalizada, cancellationToken);

        if (!comisionEstandar && !comisionConfigurada)
        {
            return EstadoReasignacionComision.ComisionInexistente;
        }

        if (inscripcion.Section == seccionNormalizada)
            return EstadoReasignacionComision.YaAsignada;

        inscripcion.Section = seccionNormalizada;
        await studentRepository.SaveChangesAsync(cancellationToken);

        return EstadoReasignacionComision.Reasignada;
    }
}
