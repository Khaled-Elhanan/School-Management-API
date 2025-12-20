using Application.Tenancy;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Constants;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tenacy;



public class TenantService : ITenantService
{
    private readonly IMultiTenantStore<ABCSchoolTenantInfo> _tenantStore;
    private readonly ApplicationDbSeeder _dbSeeder;
    private readonly IServiceProvider _serviceProvider;
    

    public TenantService(IMultiTenantStore<ABCSchoolTenantInfo> tenantStore, ApplicationDbSeeder dbSeeder, IServiceProvider serviceProvider)
    {
        _tenantStore = tenantStore;
        _dbSeeder = dbSeeder;
        _serviceProvider = serviceProvider;
    }
    
    public async Task<string> ActivateTenantAsync(string id)
    {
        var tenantInDb = await _tenantStore.TryGetAsync(id);
        tenantInDb.IsActive = true;
        await _tenantStore.TryUpdateAsync(tenantInDb);
        return tenantInDb.Identifier;
    }
    
    public async Task<string> CreateTenantAsync(CreateTenantRequest createTenant, CancellationToken cancellationToken)
    {
        var newTenant = new ABCSchoolTenantInfo
        {
            Identifier = createTenant.Identifier,
            IsActive = createTenant.IsActive,
            Name = createTenant.Name,
            ConnectionString = createTenant.ConnectionString,
            Email = createTenant.Email,
            FirstName = createTenant.FirstName,
            LastName = createTenant.LastName,
            ValidUpTo = createTenant.ValidUpTo,
           
        };
        await _tenantStore.TryAddAsync(newTenant);  
        // Seeding tenant data
        using var scope = _serviceProvider.CreateScope();
        _serviceProvider.GetRequiredService<IMultiTenantContextSetter>()
            .MultiTenantContext= new MultiTenantContext<ABCSchoolTenantInfo>()
        {
            TenantInfo = newTenant,
        };
        await scope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>()
            .InitalizeDatabaseAsync(cancellationToken); 
        return newTenant.Identifier;
    }

   

    public async Task<string> DeactivateTenantAsync(string tenantId)
    {
        var tenantInDb = await _tenantStore.TryGetAsync(tenantId);
        tenantInDb.IsActive = false;
        await _tenantStore.TryUpdateAsync(tenantInDb);
        return tenantInDb.Identifier;
        
    }
    public async Task<TenantResponse> GetTenantByIdAsync(string tenantId)
    {
        var tenantInDb = await _tenantStore.TryGetAsync(tenantId);

        #region Manual Mapping --> without using Mapster
        //var tenantResponse = new TenantResponse
        // {
        //     Identifier = tenantInDb.Identifier,
        //     IsActive = tenantInDb.IsActive,
        //     Name = tenantInDb.Name,
        //     ConnectionString = tenantInDb.ConnectionString,
        //     Email = tenantInDb.Email,
        //     FirstName = tenantInDb.FirstName,
        //     LastName = tenantInDb.LastName,
        //     ValidUpTo = tenantInDb.ValidUpTo,
        //
        // };

        #endregion 
        
        // Using Mapster
        return tenantInDb.Adapt<TenantResponse>();


    }

  
    public async Task<List<TenantResponse>> GetTenantsAsync()
    {
      var tenantsInDb = await _tenantStore.GetAllAsync();
      return tenantsInDb.Adapt<List<TenantResponse>>();
    }

    public async Task<string> UpdateSubscriptionAsync(UpdateTenantSubscriptionRequest updateTenantSubscription)
    {
        var tenantInDb = await _tenantStore.TryGetAsync(updateTenantSubscription.TenantId);
        tenantInDb.ValidUpTo = updateTenantSubscription.NewExpiryDate;
        await _tenantStore.TryUpdateAsync(tenantInDb);
        return tenantInDb.Identifier;
    }

}