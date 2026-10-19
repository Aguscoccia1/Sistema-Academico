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

    public string CreateToken(User user)
    {
        string? key = _configuration["Jwt:Key"];

        if (key == null)
        {
            throw new InvalidOperationException("Falta 'Jwt:Key'. Configurala con user-secrets.");
        }

        // el token vence a los 30 minutos (se puede cambiar en appsettings.json)
        int expiresMinutes = 30;
        string? configuredMinutes = _configuration["Jwt:ExpiresMinutes"];

        if (configuredMinutes != null)
        {
            expiresMinutes = int.Parse(configuredMinutes);
        }

        // datos que lleva el token: id del usuario y su rol
        List<Claim> claims = new List<Claim>
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("role", user.Role.Name)
        };

        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}