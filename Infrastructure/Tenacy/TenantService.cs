using Application.Tenancy;
using Application.Exceptions;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Constants;
using Mapster;
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
    
    public async Task<string> CreateTenantAsync(CreateTenantRequest createTenant, CancellationToken cancellationToken)
    {
        // Check if tenant with this identifier already exists
        if (!string.IsNullOrWhiteSpace(createTenant.Identifier))
        {
            var existingTenant = await _tenantStore.TryGetAsync(createTenant.Identifier);
            if (existingTenant != null)
            {
                throw new ConflictException(
                    new List<string> { $"A tenant with identifier '{createTenant.Identifier}' already exists." },
                    HttpStatusCode.Conflict
                );
            }
        }

        var newTenant = new ABCSchoolTenantInfo
        {
            // Ensure primary key is set; Finbuckle requires non-null Id
            Id = string.IsNullOrWhiteSpace(createTenant.Identifier) 
                ? Guid.NewGuid().ToString() 
                : createTenant.Identifier,
            Identifier = createTenant.Identifier,
            IsActive = createTenant.IsActive,
            Name = createTenant.Name,
            ConnectionString = createTenant.ConnectionString,
            Email = createTenant.Email,
            FirstName = createTenant.FirstName,
            LastName = createTenant.LastName,
            ValidUpTo = createTenant.ValidUpTo,
           
        };
        
        try
        {
            await _tenantStore.TryAddAsync(newTenant);
        }
        catch (Exception ex) when (ex.Message.Contains("duplicate key") || ex.Message.Contains("PRIMARY KEY"))
        {
            throw new ConflictException(
                new List<string> { $"A tenant with identifier '{createTenant.Identifier}' already exists." },
                HttpStatusCode.Conflict
            );
        }  
        
        // Seeding tenant data (only if connection string is provided)
        if (!string.IsNullOrWhiteSpace(newTenant.ConnectionString))
        {
            using var scope = _serviceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>()
                .MultiTenantContext= new MultiTenantContext<ABCSchoolTenantInfo>()
            {
                TenantInfo = newTenant,
            };
            await scope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>()
                .InitalizeDatabaseAsync(cancellationToken);
        }
        
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