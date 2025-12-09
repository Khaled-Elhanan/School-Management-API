using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tenacy;

public class TenantDbSeeder :ITenantDbSeeder
{
    private readonly TenantDbContext _tenantDbContext;
    private readonly IServiceProvider _serviceProvider;

    public TenantDbSeeder(TenantDbContext tenantDbContext, IServiceProvider serviceProvider)
    {
        _tenantDbContext = tenantDbContext;
        _serviceProvider = serviceProvider;
    }
    public async Task IntializeDatabaseAsync(CancellationToken cancellationToken)
    {
        // Seed Tenant data
        await IntializeTenantDbAsync(cancellationToken);
        foreach (var tenant  in await _tenantDbContext.TenantInfo.ToListAsync(cancellationToken))
        {
            // Application Db Seeder
            await InitializeApplicationDbForTenantAsync(tenant, cancellationToken);
        }
        
    }

    private async Task IntializeTenantDbAsync(CancellationToken cancellationToken)
    {
        if(await _tenantDbContext.TenantInfo.FindAsync((TenancyConstants.Root.Id),cancellationToken)  is null)
        {
            // Create tenant
            var rootTenant = new ABCSchoolTenantInfo()
            {
                Id = TenancyConstants.Root.Id,
                Identifier = TenancyConstants.Root.Id,
                Name = TenancyConstants.Root.Name,
                Email = TenancyConstants.Root.Email,
                FirstName = TenancyConstants.FirstName,
                LastName = TenancyConstants.LastName, 
                IsActive = true,
                ValidUpTo = DateTime.UtcNow.AddYears(2)
            };
            await _tenantDbContext.TenantInfo.AddAsync(rootTenant , cancellationToken);
            await _tenantDbContext.SaveChangesAsync(cancellationToken);

        }
    }
    private async Task InitializeApplicationDbForTenantAsync(ABCSchoolTenantInfo currentTenant , CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>()
            .MultiTenantContext = new MultiTenantContext<ABCSchoolTenantInfo>()
        {
                TenantInfo = currentTenant,
        };
        await scope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>()
            .InitalizeDatabaseAsync(cancellationToken); 
        
    }
}