using AcademicSystem.Data.Context;
using AcademicSystem.Business.Services;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = RoleNames.AcademicSecretary)]
public class UsersController(AcademicSystemContext context, IUserService userService) : ControllerBase
{
    private readonly PasswordHasher<User> passwordHasher = new();

    [HttpGet]
    public async Task<ActionResult<List<UserResponseDto>>> GetAll(CancellationToken ct) =>
        Ok(await userService.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponseDto>> GetById(int id, CancellationToken ct)
    {
        var user = await userService.GetByIdAsync(id, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserInput input, CancellationToken ct)
    {
        var error = Validate(input);
        if (error is not null) return BadRequest(new { message = error });
        if (!await context.Roles.AnyAsync(x => x.Id == input.RoleId, ct)) return BadRequest(new { message = "El rol indicado no existe." });
        var cuil = input.Cuil.Trim(); var email = input.Email.Trim();
        if (await context.Users.AnyAsync(x => x.Cuil == cuil || x.Email == email, ct)) return Conflict(new { message = "Ya existe un usuario con ese CUIL o correo." });
        var user = new User { FirstName = input.FirstName.Trim(), LastName = input.LastName.Trim(), Cuil = cuil,
            BirthDate = input.BirthDate, Address = Clean(input.Address), RoleId = input.RoleId, Email = email,
            Phone = Clean(input.Phone), EmergencyContactPhone = Clean(input.EmergencyContactPhone), IsActive = true };
        user.PasswordHash = passwordHasher.HashPassword(user, input.Password!);
        context.Users.Add(user);
        await context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new { user.Id, user.FirstName, user.LastName, user.Cuil,
            user.BirthDate, user.Address, user.RoleId, user.Email, user.Phone, user.EmergencyContactPhone, user.IsActive });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UserInput input, CancellationToken ct)
    {
        var error = Validate(input, true);
        if (error is not null) return BadRequest(new { message = error });
        var user = await context.Users.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (user is null) return NotFound();
        var roleName = await context.Roles.Where(x => x.Id == input.RoleId).Select(x => x.Name).SingleOrDefaultAsync(ct);
        if (roleName is null) return BadRequest(new { message = "El rol indicado no existe." });
        var profile = await context.Users.Where(x => x.Id == id)
            .Select(x => new { HasStudent = x.Student != null, HasSecretary = x.AcademicSecretary != null }).SingleAsync(ct);
        if (profile.HasStudent && roleName != RoleNames.Student)
            return Conflict(new { message = "El usuario tiene perfil de estudiante y debe conservar el rol Student." });
        if (profile.HasSecretary && roleName != RoleNames.AcademicSecretary)
            return Conflict(new { message = "El usuario tiene perfil de secretario académico y debe conservar el rol AcademicSecretary." });
        var dejaDeSerSecretarioActivo = !input.IsActive || roleName != RoleNames.AcademicSecretary;
        if (dejaDeSerSecretarioActivo && await IsLastActiveSecretary(user, ct))
            return Conflict(new { message = "No se puede desactivar ni cambiar de rol al último secretario activo." });
        var cuil = input.Cuil.Trim(); var email = input.Email.Trim();
        if (await context.Users.AnyAsync(x => x.Id != id && (x.Cuil == cuil || x.Email == email), ct)) return Conflict(new { message = "Otro usuario ya tiene ese CUIL o correo." });
        user.FirstName = input.FirstName.Trim(); user.LastName = input.LastName.Trim(); user.Cuil = cuil;
        user.BirthDate = input.BirthDate; user.Address = Clean(input.Address); user.RoleId = input.RoleId;
        user.Email = email; user.Phone = Clean(input.Phone); user.EmergencyContactPhone = Clean(input.EmergencyContactPhone); user.IsActive = input.IsActive;
        if (!string.IsNullOrWhiteSpace(input.Password)) user.PasswordHash = passwordHasher.HashPassword(user, input.Password);
        await context.SaveChangesAsync(ct);
        return Ok(new { user.Id, user.FirstName, user.LastName, user.Cuil, user.BirthDate, user.Address, user.RoleId,
            user.Email, user.Phone, user.EmergencyContactPhone, user.IsActive });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var user = await context.Users.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (user is null) return NotFound();
        if (await IsLastActiveSecretary(user, ct))
            return Conflict(new { message = "No se puede dar de baja al último secretario activo." });
        user.IsActive = false;
        await context.SaveChangesAsync(ct);
        return NoContent();
    }

    private static string? Validate(UserInput x, bool passwordOptional = false)
    {
        if (string.IsNullOrWhiteSpace(x.FirstName) || string.IsNullOrWhiteSpace(x.LastName)) return "Nombre y apellido son obligatorios.";
        var cuil = x.Cuil?.Trim() ?? "";
        if (cuil.Length != 11 || cuil.Any(c => !char.IsDigit(c))) return "El CUIL debe contener 11 dígitos.";
        if (string.IsNullOrWhiteSpace(x.Email) || x.Email.Trim().Length > 255 || !x.Email.Contains('@')) return "Ingresá un correo válido de hasta 255 caracteres.";
        if (!passwordOptional && string.IsNullOrWhiteSpace(x.Password)) return "La contraseña es obligatoria.";
        return null;
    }
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private async Task<bool> IsLastActiveSecretary(User user, CancellationToken ct)
    {
        var esSecretario = await context.Roles.AnyAsync(r => r.Id == user.RoleId && r.Name == RoleNames.AcademicSecretary, ct);
        if (!esSecretario || !user.IsActive) return false;
        var otrosActivos = await context.Users.CountAsync(u => u.Id != user.Id && u.IsActive
            && u.Role.Name == RoleNames.AcademicSecretary, ct);
        return otrosActivos == 0;
    }

    public sealed record UserInput(string FirstName, string LastName, string Cuil, DateOnly? BirthDate, string? Address,
        int RoleId, string Email, string? Phone, string? EmergencyContactPhone, string? Password, bool IsActive = true);
}
