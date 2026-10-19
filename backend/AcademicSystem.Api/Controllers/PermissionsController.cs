
using AcademicSystem.Business.Services;
using AcademicSystem.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.Api.Controllers;

/// <summary>
/// Devuelve los módulos y acciones permitidos para el rol del usuario autenticado.
/// El frontend lo usa para armar el menú. GET /api/auth/permissions
/// </summary>
[ApiController]
[Route("api/auth")]
public class PermissionsController(IPermissionService permissionService) : ControllerBase
{
    [HttpGet("permissions")]
    public ActionResult<PermissionsResponseDto> Get()
    {
        // El rol viaja en el claim "role" (Program.cs usa MapInboundClaims = false).
        var role = User.FindFirst("role")?.Value;
        var permissions = role is null ? null : permissionService.GetByRole(role);

       
        return permissions is null ? Forbid() : Ok(permissions);
    }
}