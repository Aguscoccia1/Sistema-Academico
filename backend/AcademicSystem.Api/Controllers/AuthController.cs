//(damian)
using AcademicSystem.Api.Security;
using AcademicSystem.Business.Services;
using AcademicSystem.Entities.DTOs;
using AcademicSystem.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IJwtTokenService tokenService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _tokenService = tokenService;
        _logger = logger;
    }

    // es la unica ruta publica de la API, el resto exige token
    // se utilizan DTO para que nunca se devuelvan datos sensibles(como el hash de la contraseña)
    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDto loginRequest)
    {
        try
        {
            if (loginRequest == null
                || string.IsNullOrWhiteSpace(loginRequest.Email)
                || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return BadRequest("Debe ingresar email y contraseña.");
            }

            User? user = _authService.Login(loginRequest.Email, loginRequest.Password);

            if (user == null)
            {
                // solo se loguea el email, nunca la contraseña
                _logger.LogWarning("Intento de inicio de sesion fallido para {Email}", loginRequest.Email);
                return Unauthorized("Correo o contraseña incorrectos");
            }

            LoginResponseDto loginResponse = new LoginResponseDto
            {
                Id = user.Id,
                FullName = user.FirstName + " " + user.LastName,
                Email = user.Email,
                Role = user.Role.Name,
                Token = _tokenService.CreateToken(user)
            };

           // solo los estudiantes tienen un legajo
            if (user.Student != null)
            {
                loginResponse.RecordNumber = user.Student.RecordNumber;
            }

            return Ok(loginResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado durante el inicio de sesión");
            return StatusCode(500, "Ocurrió un error inesperado. Intentá de nuevo más tarde.");
        }
    }
}
