using AcademicSystem.Business.Services;
using AcademicSystem.Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/prerequisites")]
[Authorize(Roles = RoleNames.Student)]
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
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Validate(
        [FromBody] PrerequisiteValidationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(User.FindFirst("sub")?.Value, out int userId))
        {
            return Unauthorized("No se pudo identificar al usuario autenticado.");
        }

        try
        {
            var result = await _validationService.ValidateAsync(userId, request, cancellationToken);

            if (result is null)
            {
                return NotFound("No existe el perfil de estudiante autenticado, el plan de estudios o la materia indicada.");
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
