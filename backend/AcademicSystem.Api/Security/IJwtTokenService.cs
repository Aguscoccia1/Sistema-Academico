using AcademicSystem.Entities.Models;

namespace AcademicSystem.Api.Security;

public interface IJwtTokenService
{
    int ExpiresMinutes { get; }

    string CreateToken(User user);

    string CreateToken(int userId, string roleName);
}