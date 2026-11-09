using AcademicSystem.Data.Repositories;
using AcademicSystem.Entities.DTOs;

namespace AcademicSystem.Business.Services;

/// <summary>
/// Checks that the database is reachable.
/// Example of the pattern every team follows: Controller (Api) -> Service (Business) -> Repository (Data) -> Database.
/// </summary>
public class HealthService(IHealthRepository healthRepository) : IHealthService
{
    public async Task<HealthResponseDto> CheckAsync(CancellationToken cancellationToken = default)
    {
        if (!await healthRepository.CanConnectAsync(cancellationToken))
        {
            return new HealthResponseDto("error", "unreachable", null);
        }

        var roles = await healthRepository.CountRolesAsync(cancellationToken);

        return new HealthResponseDto("ok", healthRepository.GetDatabaseName(), roles);
    }
}
