using AcademicSystem.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AcademicSystem.Data.Repositories;

public class HealthRepository(AcademicSystemContext context) : IHealthRepository
{
    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) =>
        context.Database.CanConnectAsync(cancellationToken);

    public Task<int> CountRolesAsync(CancellationToken cancellationToken = default) =>
        context.Roles.CountAsync(cancellationToken);

    public string GetDatabaseName() => context.Database.GetDbConnection().Database;
}
