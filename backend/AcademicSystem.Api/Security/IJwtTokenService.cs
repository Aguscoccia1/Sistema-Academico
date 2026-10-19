using AcademicSystem.Entities.Models;

namespace AcademicSystem.Api.Security;

public interface IJwtTokenService
{
    string CreateToken(User user);
}