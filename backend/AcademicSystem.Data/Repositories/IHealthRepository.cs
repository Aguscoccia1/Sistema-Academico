namespace AcademicSystem.Data.Repositories;

public interface IHealthRepository
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);

    Task<int> CountRolesAsync(CancellationToken cancellationToken = default);

    string GetDatabaseName();
}
