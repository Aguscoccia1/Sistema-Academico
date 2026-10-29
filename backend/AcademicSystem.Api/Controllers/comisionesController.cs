using AcademicSystem.Business.Services;
using AcademicSystem.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/comisiones")]
public class ComisionesController(IComisionesService comisionesService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ComisionResponseDto>>> GetComisiones(
        [FromQuery] int? materiaId,
        CancellationToken cancellationToken)
    {
        if (materiaId is <= 0)
            return BadRequest("El ID de la materia debe ser válido.");

        var comisiones = await comisionesService.ObtenerTodasAsync(
            materiaId, cancellationToken);

        return Ok(comisiones);
    }

    [HttpGet("{seccion}/estudiantes/{estudianteId:int}")]
    public async Task<ActionResult<EstudianteResponseDto>> BuscarEstudiante(
        string seccion,
        int estudianteId,
        [FromQuery] int materiaId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(seccion) ||
            estudianteId <= 0 ||
            materiaId <= 0)
        {
            return BadRequest("La comisión, la materia y el estudiante son obligatorios.");
        }

        var estudiante = await comisionesService.BuscarEstudianteAsync(
            materiaId, seccion, estudianteId, cancellationToken);

        return estudiante is null
            ? NotFound("El estudiante no pertenece a esa comisión.")
            : Ok(estudiante);
    }

    [HttpPost("asignacion-aleatoria-masiva")]
    public async Task<ActionResult<AsignacionAleatoriaMasivaResponseDto>> AsignarAleatoriamente(
        [FromBody] AsignacionAleatoriaMasivaRequestDto solicitud,
        CancellationToken cancellationToken)
    {
        var resultado = await comisionesService.AsignarAleatoriamenteAsync(
            solicitud.MateriaId, cancellationToken);

        if (resultado is null)
            return NotFound("No se encontró la materia.");

        return resultado.Exito
            ? Ok(resultado)
            : BadRequest(resultado.Mensaje);
    }

}
