using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tenacy;

public class TenantDbSeeder :ITenantDbSeeder
{
    private readonly TenantDbContext _tenantDbContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public TenantDbSeeder(TenantDbContext tenantDbContext, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _tenantDbContext = tenantDbContext;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
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
        var rootTenant = await _tenantDbContext.TenantInfo.FindAsync(new object[] { TenancyConstants.Root.Id }, cancellationToken);
        if(rootTenant is null)
        {
            // Get default connection string for root tenant
            var defaultConnectionString = _configuration.GetConnectionString("DefaultConnection");
            
            // Create tenant
            rootTenant = new ABCSchoolTenantInfo()
            {
                Id = TenancyConstants.Root.Id,
                Identifier = TenancyConstants.Root.Id,
                Name = TenancyConstants.Root.Name,
                Email = TenancyConstants.Root.Email,
                FirstName = TenancyConstants.FirstName,
                LastName = TenancyConstants.LastName,
                ConnectionString = defaultConnectionString, // Set default connection string for root tenant
                IsActive = true,
                ValidUpTo = DateTime.UtcNow.AddYears(2)
            };
            await _tenantDbContext.TenantInfo.AddAsync(rootTenant , cancellationToken);
            await _tenantDbContext.SaveChangesAsync(cancellationToken);
        }
        else if (string.IsNullOrEmpty(rootTenant.ConnectionString))
        {
            // Update existing root tenant with connection string if missing
            var defaultConnectionString = _configuration.GetConnectionString("DefaultConnection");
            rootTenant.ConnectionString = defaultConnectionString;
            await _tenantDbContext.SaveChangesAsync(cancellationToken);
        }
    }
    private async Task InitializeApplicationDbForTenantAsync(ABCSchoolTenantInfo currentTenant , CancellationToken cancellationToken)
    {
        // Ensure tenant has a connection string (use default if not set)
        if (string.IsNullOrWhiteSpace(currentTenant.ConnectionString))
        {
            var defaultConnectionString = _configuration.GetConnectionString("DefaultConnection");
            currentTenant.ConnectionString = defaultConnectionString;
            // Entity is already tracked, just save changes
            await _tenantDbContext.SaveChangesAsync(cancellationToken);
        }
        
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