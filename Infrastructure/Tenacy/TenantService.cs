using Application.Exceptions;
using Application.Tenancy;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Constants;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;

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

    public async Task<string> CreateTenantAsync(
    CreateTenantRequest createTenant,
    CancellationToken cancellationToken)
    {
        // Check duplicate identifier
        if (!string.IsNullOrWhiteSpace(createTenant.Identifier))
        {
            var existingTenant = await _tenantStore.TryGetAsync(createTenant.Identifier);
            if (existingTenant != null)
            {
                throw new ConflictException(
                    new List<string> { $"A tenant with identifier '{createTenant.Identifier}' already exists." },
                    HttpStatusCode.Conflict);
            }
        }

        // ?? Handle connection string properly
        var connectionString =
            string.IsNullOrWhiteSpace(createTenant.ConnectionString) ||
            createTenant.ConnectionString == "string"
                ? _serviceProvider
                    .GetRequiredService<IConfiguration>()
                    .GetConnectionString("DefaultConnection")
                : createTenant.ConnectionString;

        var newTenant = new ABCSchoolTenantInfo
        {
            Id = string.IsNullOrWhiteSpace(createTenant.Identifier)
                ? Guid.NewGuid().ToString()
                : createTenant.Identifier,

            Identifier = createTenant.Identifier,
            IsActive = createTenant.IsActive,
            Name = createTenant.Name,
            ConnectionString = connectionString,
            Email = createTenant.Email,
            FirstName = createTenant.FirstName,
            LastName = createTenant.LastName,
            ValidUpTo = createTenant.ValidUpTo
        };

        await _tenantStore.TryAddAsync(newTenant);

        // ?? IMPORTANT: run seeder INSIDE tenant context
        using var scope = _serviceProvider.CreateScope();

        var tenantContextSetter =
            scope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>();

        tenantContextSetter.MultiTenantContext =
            new MultiTenantContext<ABCSchoolTenantInfo>
            {
                TenantInfo = newTenant
            };

        await scope.ServiceProvider
            .GetRequiredService<ApplicationDbSeeder>()
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