using System.Text.Json;
using AcademicSystem.Business.Services;
using AcademicSystem.Data.Context;
using AcademicSystem.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/academic-secretaries")]
[Authorize(Roles = RoleNames.AcademicSecretary)]
public class AcademicSecretariesController(AcademicSystemContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await context.AcademicSecretaries.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.UserId, x.TitlesJson }).ToListAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var secretary = await context.AcademicSecretaries.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.Id, x.UserId, x.TitlesJson }).SingleOrDefaultAsync(ct);
        return secretary is null ? NotFound() : Ok(secretary);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AcademicSecretary input, CancellationToken ct)
    {
        var error = ValidateTitles(input.TitlesJson);
        if (error is not null) return BadRequest(new { message = error });
        var userRole = await context.Users.Where(x => x.Id == input.UserId).Select(x => x.Role.Name).SingleOrDefaultAsync(ct);
        if (userRole is null) return BadRequest(new { message = "El usuario indicado no existe." });
        if (userRole != RoleNames.AcademicSecretary) return BadRequest(new { message = "El usuario debe tener el rol AcademicSecretary para crear un perfil de secretario académico." });
        if (await context.AcademicSecretaries.AnyAsync(x => x.UserId == input.UserId, ct))
            return Conflict(new { message = "Ese usuario ya tiene perfil de secretario académico." });
        var secretary = new AcademicSecretary { UserId = input.UserId, TitlesJson = Clean(input.TitlesJson) };
        context.AcademicSecretaries.Add(secretary);
        await context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = secretary.Id }, new { secretary.Id, secretary.UserId, secretary.TitlesJson });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AcademicSecretary input, CancellationToken ct)
    {
        var error = ValidateTitles(input.TitlesJson);
        if (error is not null) return BadRequest(new { message = error });
        var secretary = await context.AcademicSecretaries.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (secretary is null) return NotFound();
        secretary.TitlesJson = Clean(input.TitlesJson);
        await context.SaveChangesAsync(ct);
        return Ok(new { secretary.Id, secretary.UserId, secretary.TitlesJson });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var secretary = await context.AcademicSecretaries.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (secretary is null) return NotFound();
        context.AcademicSecretaries.Remove(secretary);
        await context.SaveChangesAsync(ct);
        return NoContent();
    }

    private static string? ValidateTitles(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { using var _ = JsonDocument.Parse(json); return null; }
        catch (JsonException) { return "TitlesJson debe contener JSON válido."; }
    }
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
