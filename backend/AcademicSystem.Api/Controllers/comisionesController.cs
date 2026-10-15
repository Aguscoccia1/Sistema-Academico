
using Microsoft.AspNetCore.Mvc;

namespace SistemaAcademico.API.Controllers
{
    [ApiController]
    [Route("api/comisiones")]
    public class ComisionesController : ControllerBase
    {
        private readonly IComisionesService _comisionesService;

        public ComisionesController(
            IComisionesService comisionesService)
        {
            _comisionesService = comisionesService;
        }

        // GET: api/comisiones
        [HttpGet]
        public async Task<IActionResult> GetComisiones()
        {
            var comisiones =
                await _comisionesService.ObtenerTodasAsync();

            return Ok(comisiones);
        }

        // GET: api/comisiones/3/estudiantes/12
        [HttpGet("{comisionId:int}/estudiantes/{estudianteId:int}")]
        public async Task<IActionResult> BuscarEstudiante(
            int comisionId,
            int estudianteId)
        {
            if (comisionId <= 0 || estudianteId <= 0)
                return BadRequest("Los IDs deben ser válidos.");

            var estudiante =
                await _comisionesService.BuscarEstudianteAsync(
                    comisionId, estudianteId);

            if (estudiante == null)
                return NotFound(
                    "El estudiante no pertenece a esa comisión.");

            return Ok(estudiante);
        }
    }
}
