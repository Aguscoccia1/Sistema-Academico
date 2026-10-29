namespace AcademicSystem.Api.Security;

public sealed record RefreshTokenInfo(int UserId, DateTimeOffset LastActivity, DateTimeOffset ExpiresAt);

public interface IRefreshTokenService
{
    TimeSpan InactivityWindow { get; }

    string Create(int userId, DateTimeOffset lastActivity, DateTimeOffset expiresAt);

    RefreshTokenInfo? Validate(string token);

    DateTimeOffset NewExpiration(bool remember);
}
