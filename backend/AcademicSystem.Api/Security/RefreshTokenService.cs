using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AcademicSystem.Api.Security;

public class RefreshTokenService : IRefreshTokenService
{
    private const string Audience = "AcademicSystem.Refresh";

    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _key;

    public RefreshTokenService(IConfiguration configuration)
    {
        _configuration = configuration;

        string? key = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Falta 'Jwt:Key'. Configurala con user-secrets.");
        }

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }

    public TimeSpan InactivityWindow =>
        TimeSpan.FromMinutes(_configuration.GetValue("Jwt:SessionInactivityMinutes", 30));

    public DateTimeOffset NewExpiration(bool remember) =>
        DateTimeOffset.UtcNow.Add(remember
            ? TimeSpan.FromDays(_configuration.GetValue("Jwt:SessionRememberDays", 7))
            : TimeSpan.FromHours(_configuration.GetValue("Jwt:SessionAbsoluteHours", 12)));

    public string Create(int userId, DateTimeOffset lastActivity, DateTimeOffset expiresAt)
    {
        Claim[] claims =
        [
            new Claim("sub", userId.ToString()),
            new Claim("act", lastActivity.ToUnixTimeSeconds().ToString())
        ];

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshTokenInfo? Validate(string token)
    {
        try
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler
            {
                MapInboundClaims = false
            };

            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            JwtSecurityToken jwt = (JwtSecurityToken)validatedToken;

            string? sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            string? act = jwt.Claims.FirstOrDefault(c => c.Type == "act")?.Value;

            if (!int.TryParse(sub, out int userId) || !long.TryParse(act, out long activitySeconds))
            {
                return null;
            }

            return new RefreshTokenInfo(
                userId,
                DateTimeOffset.FromUnixTimeSeconds(activitySeconds),
                jwt.ValidTo);
        }
        catch
        {
            return null;
        }
    }
}
