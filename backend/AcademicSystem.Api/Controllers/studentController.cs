
using AcademicSystem.Business.Services;
using AcademicSystem.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/estudiantes")]
public class StudentController(IStudentService estudiantesService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EstudianteResponseDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequest("El ID no es válido.");

        var estudiante = await estudiantesService.ObtenerPorIdAsync(id, cancellationToken);

        return estudiante is null
            ? NotFound("No se encontró el estudiante.")
            : Ok(estudiante);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Modificar(
        int id,
        [FromBody] ModificarEstudianteDto datos,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequest("El ID no es válido.");

        var resultado = await estudiantesService.ModificarAsync(
            id, datos, cancellationToken);

        return resultado switch
        {
            EstadoModificacionEstudiante.Actualizado => NoContent(),
            EstadoModificacionEstudiante.DniDuplicado =>
                Conflict("Ya existe otro estudiante con ese DNI."),
            _ => NotFound("No se encontró el estudiante.")
        };
    }

    [HttpPut("{id:int}/comision")]
    public async Task<IActionResult> ReasignarComision(
        int id,
        [FromBody] ReasignarComisionDto datos,
        CancellationToken cancellationToken)
    {
        if (id <= 0 || datos.MateriaId <= 0 ||
            string.IsNullOrWhiteSpace(datos.Seccion))
        {
            return BadRequest("El estudiante, la materia y la comisión son obligatorios.");
        }

        var resultado = await estudiantesService.ReasignarComisionAsync(
            id, datos.MateriaId, datos.Seccion, cancellationToken);

        return resultado switch
        {
            EstadoReasignacionComision.Reasignada =>
                Ok(new { mensaje = "Comisión reasignada correctamente." }),
            EstadoReasignacionComision.YaAsignada =>
                Ok(new { mensaje = "El estudiante ya pertenece a esa comisión." }),
            EstadoReasignacionComision.ComisionInexistente =>
                NotFound("La comisión no existe para esa materia."),
            _ => NotFound("No se encontró la inscripción del estudiante en esa materia.")
        };
    }
}
