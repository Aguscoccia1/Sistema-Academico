using Microsoft.AspNetCore.Mvc;
using NuevaApi;
using SistemaInscripcion;

namespace ApiStudent.Controllers;

[ApiController]
[Route("[Controller]")]
public class ResgistrationController : ControllerBase
{

    private readonly ILogger<ResgistrationController> _logger;

    public ResgistrationController(ILogger<ResgistrationController> logger)
    {
        _logger = logger;
    }

    // GET: api/inscripciones/estudiantes
    [HttpGet("estudiantes")]
    public async Task<IActionResult> GetEstudiantes()
    {
        var estudiantes = await _inscripcionesService.ObtenerEstudiantesAsync();

        return Ok(estudiantes);
    }

    // GET: api/inscripciones/materias?anio=1
    [HttpGet("materias")]
    public async Task<IActionResult> GetMaterias([FromQuery] int anio = 1)
    {
        if (anio < 1)
            return BadRequest("El año académico debe ser válido.");

        var materias =
            await _inscripcionesService.ObtenerMateriasPorAnioAsync(anio);

        return Ok(materias);
    }

    // POST: api/inscripciones
    [HttpPost]
    public async Task<IActionResult> CrearInscripcion(
        [FromBody] CrearInscripcionDto solicitud)
    {
        if (solicitud.EstudianteId <= 0 || solicitud.MateriaId <= 0)
        {
            return BadRequest(
                "El estudiante y la materia son obligatorios.");
        }

        var resultado =
            await _inscripcionesService.InscribirAsync(solicitud);

        if (!resultado.Exito)
            return BadRequest(resultado.Mensaje);

        return Ok(resultado);
    }

    // GET: api/inscripciones/estudiante/12
    [HttpGet("estudiante/{id:int}")]
    public async Task<IActionResult> GetPorEstudiante(int id)
    {
        if (id <= 0)
            return BadRequest("El ID del estudiante no es válido.");

        var inscripciones =
            await _inscripcionesService
                .ObtenerPorEstudianteAsync(id);

        return Ok(inscripciones);
    }
    // GET: api/inscripciones/cantidad
    [HttpGet("cantidad")]
    public async Task<IActionResult> GetCantidadInscriptos()
    {
        var cantidad =
            await _inscripcionesService.ObtenerCantidadAsync();

        return Ok(new
        {
            totalInscriptos = cantidad
        });
    }

}