using AcademicSystem.Data.Context;
using AcademicSystem.Business.Services;
using AcademicSystem.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/students")]
[Authorize(Roles = RoleNames.AcademicSecretary)]
public class StudentsController(AcademicSystemContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await context.Students.AsNoTracking().OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .Select(x => new { x.Id, x.UserId, x.FirstName, x.LastName, x.Dni, x.RecordNumber }).ToListAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var student = await context.Students.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.Id, x.UserId, x.FirstName, x.LastName, x.Dni, x.RecordNumber }).SingleOrDefaultAsync(ct);
        return student is null ? NotFound() : Ok(student);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Student input, CancellationToken ct)
    {
        var error = Validate(input.FirstName, input.LastName, input.Dni, input.RecordNumber);
        if (error is not null) return BadRequest(new { message = error });
        var userRole = await context.Users.Where(x => x.Id == input.UserId).Select(x => x.Role.Name).SingleOrDefaultAsync(ct);
        if (userRole is null) return BadRequest(new { message = "El usuario indicado no existe." });
        if (userRole != RoleNames.Student) return BadRequest(new { message = "El usuario debe tener el rol Student para crear un perfil de estudiante." });
        var dni = input.Dni.Trim(); var legajo = input.RecordNumber.Trim();
        if (await context.Students.AnyAsync(x => x.UserId == input.UserId || x.Dni == dni || x.RecordNumber == legajo, ct))
            return Conflict(new { message = "El usuario ya tiene perfil o el DNI/número de legajo ya existe." });
        var student = new Student { UserId = input.UserId, FirstName = input.FirstName.Trim(), LastName = input.LastName.Trim(), Dni = dni, RecordNumber = legajo };
        context.Students.Add(student);
        await context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, new { student.Id, student.UserId, student.FirstName, student.LastName, student.Dni, student.RecordNumber });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Student input, CancellationToken ct)
    {
        var error = Validate(input.FirstName, input.LastName, input.Dni, input.RecordNumber);
        if (error is not null) return BadRequest(new { message = error });
        var student = await context.Students.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (student is null) return NotFound();
        var dni = input.Dni.Trim(); var legajo = input.RecordNumber.Trim();
        if (await context.Students.AnyAsync(x => x.Id != id && (x.Dni == dni || x.RecordNumber == legajo), ct))
            return Conflict(new { message = "Otro estudiante ya tiene ese DNI o número de legajo." });
        student.FirstName = input.FirstName.Trim(); student.LastName = input.LastName.Trim(); student.Dni = dni; student.RecordNumber = legajo;
        await context.SaveChangesAsync(ct);
        return Ok(new { student.Id, student.UserId, student.FirstName, student.LastName, student.Dni, student.RecordNumber });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var student = await context.Students.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (student is null) return NotFound();
        if (await context.StudentSubjects.AnyAsync(x => x.StudentId == id, ct))
            return Conflict(new { message = "No se puede eliminar el estudiante porque tiene historial académico asociado." });
        context.Students.Remove(student);
        await context.SaveChangesAsync(ct);
        return NoContent();
    }

    private static string? Validate(string first, string last, string dni, string legajo)
    {
        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last)) return "Nombre y apellido son obligatorios.";
        var cleanDni = dni?.Trim() ?? "";
        if (cleanDni.Length is < 7 or > 8 || cleanDni.Any(c => !char.IsDigit(c))) return "El DNI debe contener 7 u 8 dígitos.";
        if (string.IsNullOrWhiteSpace(legajo) || legajo.Trim().Length > 20) return "El número de legajo es obligatorio y admite hasta 20 caracteres.";
        return null;
    }
}
