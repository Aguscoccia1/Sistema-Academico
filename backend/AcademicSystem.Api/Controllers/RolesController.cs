using AcademicSystem.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = RoleNames.AcademicSecretary)]
public class RolesController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await roleService.GetAllAsync(ct));

    // La matriz solo permite consultar el catálogo; estas rutas siempre rechazan la acción.
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id) => Forbid();

    [HttpPost]
    public IActionResult Create() => Forbid();

    [HttpPut("{id:int}")]
    public IActionResult Update(int id) => Forbid();

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) => Forbid();
}
