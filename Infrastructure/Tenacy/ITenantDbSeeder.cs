namespace Infrastructure.Tenacy;

public interface ITenantDbSeeder
{
    Task IntializeDatabaseAsync(CancellationToken cancellationToken);
}