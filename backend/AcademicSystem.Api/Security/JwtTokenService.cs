using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AcademicSystem.Entities.Models;
using Microsoft.IdentityModel.Tokens;

namespace AcademicSystem.Api.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public int ExpiresMinutes =>
        int.TryParse(_configuration["Jwt:ExpiresMinutes"], out int minutes) ? minutes : 30;

    public string CreateToken(User user) => CreateToken(user.Id, user.Role.Name);

    public string CreateToken(int userId, string roleName)
    {
        string? key = _configuration["Jwt:Key"];

        if (key == null)
        {
            throw new InvalidOperationException("Falta 'Jwt:Key'. Configurala con user-secrets.");
        }

        // datos que lleva el token: id del usuario y su rol
        List<Claim> claims = new List<Claim>
        {
            new Claim("sub", userId.ToString()),
            new Claim("role", roleName)
        };

        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(ExpiresMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}