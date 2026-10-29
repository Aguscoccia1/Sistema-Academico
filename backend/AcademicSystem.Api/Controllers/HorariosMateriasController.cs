using AcademicSystem.Business.Services;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/horarios-materias")]
public class HorariosMateriasController(ISubjectScheduleService horariosService) : ControllerBase
{
    /// <summary>Returns the schedules configured for a subject commission.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<HorarioMateriaResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<HorarioMateriaResponseDto>>> Obtener(
        [FromQuery] int materiaId,
        [FromQuery] string seccion,
        CancellationToken cancellationToken)
    {
        if (materiaId <= 0 || string.IsNullOrWhiteSpace(seccion))
            return BadRequest("La materia y la comisión son obligatorias.");

        var resultado = await horariosService.ObtenerAsync(
            materiaId, seccion, cancellationToken);

        return resultado.Estado switch
        {
            EstadoConsultaHorarios.MateriaInexistente => NotFound("No se encontró la materia."),
            _ => Ok(resultado.Horarios)
        };
    }

    /// <summary>Creates a weekly schedule for an existing subject commission.</summary>
    [HttpPost]
    [Authorize(Roles = RoleNames.AcademicSecretary)]
    [ProducesResponseType<HorarioMateriaResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HorarioMateriaResponseDto>> Crear(
        [FromBody] CrearHorarioMateriaDto solicitud,
        CancellationToken cancellationToken)
    {
        if (solicitud.StartTime >= solicitud.EndTime)
            return BadRequest("La hora de inicio debe ser anterior a la hora de finalización.");

        var resultado = await horariosService.CrearAsync(solicitud, cancellationToken);

        if (resultado.Estado == EstadoCreacionHorario.MateriaInexistente)
            return NotFound("No se encontró la materia.");

        if (resultado.Estado == EstadoCreacionHorario.ComisionInexistente)
            return NotFound("No se encontró la comisión para esa materia.");

        if (resultado.Estado == EstadoCreacionHorario.HorarioSuperpuesto)
            return Conflict("El horario se superpone con otro horario de esa comisión.");

        return CreatedAtAction(
            nameof(Obtener),
            new { materiaId = solicitud.SubjectId, seccion = solicitud.Section },
            resultado.Horario);
    }
}
