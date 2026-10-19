using AcademicSystem.Business.Services;
using AcademicSystem.Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.Api.Controllers;

/// <summary>
/// Quick check that the API is running and can reach the database.
/// GET /api/health
/// </summary>
[ApiController]
[Route("api/health")]
[AllowAnonymous]
public class HealthController(IHealthService healthService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HealthResponseDto>> Get(CancellationToken cancellationToken)
    {
        var result = await healthService.CheckAsync(cancellationToken);

        return result.Status == "ok"
            ? Ok(result)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, result);
    }
}
