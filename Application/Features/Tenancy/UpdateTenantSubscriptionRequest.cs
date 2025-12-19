namespace Application.Tenancy;

public class UpdateTenantSubscriptionRequest
{
    public string TenantId { get; set; }
    public DateTime  NewExpiryDate { get; set; }

}