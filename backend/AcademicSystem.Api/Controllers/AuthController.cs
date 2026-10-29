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
    private readonly IUserService _userService;
    private readonly IJwtTokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IUserService userService,
        IJwtTokenService tokenService,
        IRefreshTokenService refreshTokenService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userService = userService;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    // las rutas publicas son login y refresh, el resto exige token
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

            DateTimeOffset now = DateTimeOffset.UtcNow;
            LoginResponseDto loginResponse = new LoginResponseDto
            {
                Id = user.Id,
                FullName = user.FirstName + " " + user.LastName,
                Email = user.Email,
                Role = user.Role.Name,
                Token = _tokenService.CreateToken(user),
                RefreshToken = _refreshTokenService.Create(
                    user.Id,
                    now,
                    _refreshTokenService.NewExpiration(loginRequest.Remember)),
                ExpiresIn = _tokenService.ExpiresMinutes * 60
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

    // Renueva la sesión sin tocar la base: el refresh token lleva la última actividad
    // y el tope absoluto de la sesión. Si pasaron más de 30 minutos sin actividad, no renueva.
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request, CancellationToken ct)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest("Falta el token de sesión.");
        }

        RefreshTokenInfo? info = _refreshTokenService.Validate(request.RefreshToken);
        if (info == null)
        {
            return Unauthorized("La sesión no es válida.");
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        if (now - info.LastActivity > _refreshTokenService.InactivityWindow)
        {
            return Unauthorized("La sesión expiró por inactividad.");
        }

        if (info.ExpiresAt <= now)
        {
            return Unauthorized("La sesión expiró.");
        }

        UserResponseDto? user = await _userService.GetByIdAsync(info.UserId, ct);

        if (user == null || !user.IsActive
            || (user.RoleName != RoleNames.Student && user.RoleName != RoleNames.AcademicSecretary))
        {
            return Unauthorized("La sesión no es válida.");
        }

        RefreshResponseDto response = new RefreshResponseDto
        {
            Token = _tokenService.CreateToken(user.Id, user.RoleName),
            RefreshToken = _refreshTokenService.Create(user.Id, now, info.ExpiresAt),
            ExpiresIn = _tokenService.ExpiresMinutes * 60
        };

        return Ok(response);
    }
}
