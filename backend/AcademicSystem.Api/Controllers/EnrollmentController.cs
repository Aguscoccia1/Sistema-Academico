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
}
