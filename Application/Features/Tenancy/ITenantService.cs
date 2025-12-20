using Application.Tenancy.Commands;

namespace Application.Tenancy;

public interface ITenantService
{
    Task<string>CreateTenantAsync(CreateTenantRequest createTenant , CancellationToken  cancellationToken); 
    Task<string>ActivateTenantAsync(string tenantId);
    Task<string> DeactivateTenantAsync(string tenantId);
    Task<string> UpdateSubscriptionAsync(UpdateTenantSubscriptionRequest updateTenantSubscription);
    Task<List<TenantResponse>> GetTenantsAsync();
    Task<TenantResponse> GetTenantByIdAsync(string tenantId);

}