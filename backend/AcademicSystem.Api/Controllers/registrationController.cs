using AcademicSystem.Business.Services;
using AcademicSystem.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/inscripciones")]
public class RegistrationController(IInscripcionesService inscripcionesService) : ControllerBase
{
    [HttpGet("estudiantes")]
    public async Task<ActionResult<IReadOnlyList<EstudianteResponseDto>>> GetEstudiantes(
        CancellationToken cancellationToken)
    {
        var estudiantes = await inscripcionesService.ObtenerEstudiantesAsync(cancellationToken);
        return Ok(estudiantes);
    }

    [HttpGet("materias")]
    public async Task<ActionResult<IReadOnlyList<MateriaResponseDto>>> GetMaterias(
        [FromQuery] int anio = 1,
        CancellationToken cancellationToken = default)
    {
        if (anio < 1)
            return BadRequest("El año académico debe ser válido.");

        var materias = await inscripcionesService.ObtenerMateriasPorAnioAsync(
            anio, cancellationToken);

        return Ok(materias);
    }

    [HttpPost]
    public async Task<ActionResult<ResultadoInscripcionDto>> CrearInscripcion(
        [FromBody] CrearInscripcionDto solicitud,
        CancellationToken cancellationToken)
    {
        var resultado = await inscripcionesService.InscribirAsync(
            solicitud, cancellationToken);

        return resultado.Exito
            ? Ok(resultado)
            : BadRequest(resultado);
    }

    [HttpGet("estudiante/{id:int}")]
    public async Task<ActionResult<IReadOnlyList<InscripcionResponseDto>>> GetPorEstudiante(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequest("El ID del estudiante no es válido.");

        var inscripciones = await inscripcionesService.ObtenerPorEstudianteAsync(
            id, cancellationToken);

        return Ok(inscripciones);
    }

    [HttpGet("cantidad")]
    public async Task<IActionResult> GetCantidadInscriptos(CancellationToken cancellationToken)
    {
        var cantidad = await inscripcionesService.ObtenerCantidadAsync(cancellationToken);
        return Ok(new { totalInscriptos = cantidad });
    }
}