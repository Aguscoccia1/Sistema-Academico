using AcademicSystem.Business.Services;
using AcademicSystem.Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize(Roles = RoleNames.Student)]
public class EnrollmentController(IEnrollmentService enrollmentService, ILogger<EnrollmentController> logger) : ControllerBase
{
    // materias disponibles para inscribirse
    [HttpGet("available-subjects")]
    public async Task<ActionResult<AvailableSubjectsResponseDto>> GetAvailable(CancellationToken cancellationToken)
    {
        try
        {
            var result = await enrollmentService.GetAvailableAsync(cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al consultar las materias disponibles.");
            return StatusCode(500, "Error al consultar las materias disponibles.");
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(StudentSubjectResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Enroll(
        [FromBody] StudentSubjectRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(User.FindFirst("sub")?.Value, out int userId))
        {
            return Unauthorized(new { message = "No se pudo identificar al usuario autenticado." });
        }

        try
        {
            var result = await enrollmentService.EnrollAsync(userId, request, cancellationToken);

            if (result.Status == EnrollmentResultStatus.NotFound)
            {
                return NotFound(new { message = result.Message });
            }

            if (result.Status == EnrollmentResultStatus.InvalidRequest)
            {
                return BadRequest(new { message = result.Message });
            }

            if (result.Status == EnrollmentResultStatus.PrerequisitesNotMet)
            {
                return UnprocessableEntity(new
                {
                    message = result.Message,
                    missingPrerequisites = result.Validation?.MissingPrerequisites ?? new List<PrerequisiteSubjectDto>(),
                    fulfilledPrerequisites = result.Validation?.FulfilledPrerequisites ?? new List<PrerequisiteSubjectDto>()
                });
            }

            if (result.Status == EnrollmentResultStatus.Conflict)
            {
                return Conflict(new { message = result.Message });
            }

            return StatusCode(StatusCodes.Status201Created, result.Enrollment);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al inscribir al estudiante en la materia.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error interno del servidor al realizar la inscripción." });
        }
    }
}
