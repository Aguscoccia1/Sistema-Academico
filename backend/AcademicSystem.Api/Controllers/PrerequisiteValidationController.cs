using AcademicSystem.Business.Services;
using AcademicSystem.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/prerequisites")]
public class PrerequisiteValidationController : ControllerBase
{
    private readonly IPrerequisiteValidationService _validationService;
    private readonly ILogger<PrerequisiteValidationController> _logger;

    public PrerequisiteValidationController(
        IPrerequisiteValidationService validationService,
        ILogger<PrerequisiteValidationController> logger)
    {
        _validationService = validationService;
        _logger = logger;
    }

    [HttpPost("validate")]
    [ProducesResponseType(typeof(PrerequisiteValidationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Validate(
        [FromBody] PrerequisiteValidationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // TODO: cuando E1 entregue la autenticación, obtener StudentId del usuario autenticado y no del body.
        try
        {
            var result = await _validationService.ValidateAsync(request, cancellationToken);

            if (result is null)
            {
                return NotFound("No se encontró el alumno, el plan de estudios o la materia indicados.");
            }

            return Ok(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error al validar las correlatividades del alumno.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor al validar las correlatividades.");
        }
    }
}
