using AcademicSystem.Business.Services;
using AcademicSystem.Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = RoleNames.AcademicSecretary)]
public class UsersController(IUserService userService) : ControllerBase
{
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
    public async Task<IActionResult> Create(UserRequestDto input, CancellationToken ct)
    {
        var error = Validate(input);
        if (error is not null) return BadRequest(new { message = error });
        if (!await userService.RoleExistsAsync(input.RoleId, ct)) return BadRequest(new { message = "El rol indicado no existe." });

        var cuil = input.Cuil.Trim();
        var email = input.Email.Trim();
        if (await userService.CuilOrEmailExistsAsync(cuil, email, ct)) return Conflict(new { message = "Ya existe un usuario con ese CUIL o correo." });

        var created = await userService.CreateAsync(input, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UserRequestDto input, CancellationToken ct)
    {
        var error = Validate(input, true);
        if (error is not null) return BadRequest(new { message = error });
        if (await userService.GetByIdAsync(id, ct) is null) return NotFound();

        var roleName = await userService.GetRoleNameAsync(input.RoleId, ct);
        if (roleName is null) return BadRequest(new { message = "El rol indicado no existe." });

        if (await userService.HasStudentProfileAsync(id, ct) && roleName != RoleNames.Student)
            return Conflict(new { message = "El usuario tiene perfil de estudiante y debe conservar el rol Student." });
        if (await userService.HasSecretaryProfileAsync(id, ct) && roleName != RoleNames.AcademicSecretary)
            return Conflict(new { message = "El usuario tiene perfil de secretario académico y debe conservar el rol AcademicSecretary." });

        var dejaDeSerSecretarioActivo = !input.IsActive || roleName != RoleNames.AcademicSecretary;
        if (dejaDeSerSecretarioActivo && await userService.IsLastActiveSecretaryAsync(id, ct))
            return Conflict(new { message = "No se puede desactivar ni cambiar de rol al último secretario activo." });

        var cuil = input.Cuil.Trim();
        var email = input.Email.Trim();
        if (await userService.CuilOrEmailExistsAsync(cuil, email, id, ct)) return Conflict(new { message = "Otro usuario ya tiene ese CUIL o correo." });

        var updated = await userService.UpdateAsync(id, input, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (await userService.GetByIdAsync(id, ct) is null) return NotFound();
        if (await userService.IsLastActiveSecretaryAsync(id, ct))
            return Conflict(new { message = "No se puede dar de baja al último secretario activo." });

        await userService.DeleteAsync(id, ct);
        return NoContent();
    }

    private static string? Validate(UserRequestDto x, bool passwordOptional = false)
    {
        if (string.IsNullOrWhiteSpace(x.FirstName) || string.IsNullOrWhiteSpace(x.LastName)) return "Nombre y apellido son obligatorios.";
        var cuil = x.Cuil?.Trim() ?? "";
        if (cuil.Length != 11 || cuil.Any(c => !char.IsDigit(c))) return "El CUIL debe contener 11 dígitos.";
        if (string.IsNullOrWhiteSpace(x.Email) || x.Email.Trim().Length > 255 || !x.Email.Contains('@')) return "Ingresá un correo válido de hasta 255 caracteres.";
        if (!passwordOptional && string.IsNullOrWhiteSpace(x.Password)) return "La contraseña es obligatoria.";
        return null;
    }
}
