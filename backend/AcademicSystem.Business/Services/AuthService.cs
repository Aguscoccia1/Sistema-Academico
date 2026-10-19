//(damian)
using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AcademicSystem.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher<User> _hasher = new PasswordHasher<User>();
    private readonly ILogger<AuthService> _logger;

    private static readonly TimeSpan FailedAttemptWindow = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(10);
    private const int FailedAttemptsBeforeLockout = 6;

    public AuthService(IUserRepository userRepository, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public User? Login(string email, string password)
    {
        // se busca el usuario activo por correo
        User? user = _userRepository.GetActiveByEmail(email);

        if (user == null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        if (user.LockedUntil is not null && user.LockedUntil > now)
        {
            return null;
        }

        // Al vencer la ventana o el bloqueo, comienza una nueva secuencia de intentos.
        if (user.LockedUntil is not null || user.FailedLoginWindowStartedAt is null
            || now - user.FailedLoginWindowStartedAt.Value >= FailedAttemptWindow)
        {
            user.FailedLoginAttempts = 0;
            user.FailedLoginWindowStartedAt = null;
            user.LockedUntil = null;
        }

        // la contraseña esta guardada como hash, por eso se compara con VerifyHashedPassword
        PasswordVerificationResult result =
            _hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.Failed)
        {
            user.FailedLoginWindowStartedAt ??= now;
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= FailedAttemptsBeforeLockout)
            {
                user.LockedUntil = now.Add(LockoutDuration);
                _logger.LogWarning(
                    "Cuenta bloqueada por {LockoutMinutes} minutos tras {FailedAttempts} intentos fallidos. UserId={UserId}",
                    LockoutDuration.TotalMinutes, user.FailedLoginAttempts, user.Id);
            }

            _userRepository.SaveChanges();
            return null;
        }

        // Los intentos son consecutivos; una contraseña correcta reinicia el contador.
        user.FailedLoginAttempts = 0;
        user.FailedLoginWindowStartedAt = null;
        user.LockedUntil = null;
        _userRepository.SaveChanges();

        // solo pueden entrar estudiantes y secretarios academicos
        if (user.Role.Name != RoleNames.Student && user.Role.Name != RoleNames.AcademicSecretary)
        {
            return null;
        }

        return user;
    }
}
