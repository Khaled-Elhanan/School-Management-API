using Application.Tenancy.Commands;

namespace Application.Tenancy;

public interface ITenantService
{
    Task<string>CreateTenantAsync(CreateTenantRequest createTenant , CancellationToken  cancellationToken); 
    Task<string>ActivateTenantAsync(string tenantId, CancellationToken  cancellationToken);
    Task<string> DeactivateTenantAsync(string tenantId, CancellationToken  cancellationToken);
    Task<string> UpdateSubscriptionAsync(UpdateTenantSubscriptionRequest updateTenantSubscription);
    Task<List<TenantResponse>> GetTenantsAsync(string tenantId);
    Task<TenantResponse> GetTenantByIdAsync(string tenantId);

}