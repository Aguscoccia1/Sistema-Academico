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
    
    [HttpGet("available-subjects")]
    [ProducesResponseType(typeof(AvailableSubjectsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AvailableSubjectsResponseDto>> GetAvailable(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("sub")?.Value, out int userId))
        {
            return Unauthorized(new { message = "No se pudo identificar al usuario autenticado." });
        }

        try
        {
            var result = await enrollmentService.GetAvailableAsync(userId, cancellationToken);

            if (result is null)
            {
                return NotFound(new { message = "No se encontró el perfil de estudiante del usuario autenticado." });
            }

            return Ok(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al consultar las materias disponibles.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error interno del servidor al consultar las materias disponibles." });
        }
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<StudentEnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<StudentEnrollmentDto>>> GetMyEnrollments(
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("sub")?.Value, out int userId))
        {
            return Unauthorized(new { message = "No se pudo identificar al usuario autenticado." });
        }

        try
        {
            var enrollments = await enrollmentService.GetStudentEnrollmentsAsync(userId, cancellationToken);

            if (enrollments is null)
            {
                return NotFound(new { message = "No se encontró el perfil de estudiante del usuario autenticado." });
            }

            return Ok(enrollments);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al consultar las inscripciones del estudiante autenticado.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error interno del servidor al consultar las inscripciones." });
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
