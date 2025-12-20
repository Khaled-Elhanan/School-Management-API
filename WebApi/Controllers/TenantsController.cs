using Application.Tenancy;
using Application.Tenancy.Commands;
using Application.Tenancy.Queries;
using Infrastructure.Constants;
using Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantsController : BaseApiController
    {
        [HttpPost("add")]
        [ShouldHavePermission(SchoolAction.Create , SchoolFeature.Tenants)]
        public async Task<IActionResult> CreateTenantAsync([FromBody] CreateTenantRequest createTenantRequest)
        {
            var response = await Sender.Send(new CreateTenantCommand { CreateTenant = createTenantRequest });
            if (!response.IsSuccessful)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPut("{tenantId}/activate")]
        [ShouldHavePermission(SchoolAction.Update, SchoolFeature.Tenants)]
        public async Task<IActionResult> ActivateTenantAsync([FromRoute] string tenantId)
        {
            var response = await Sender.Send(new ActivateTenantCommand{ TenantId = tenantId });
            if (!response.IsSuccessful)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        
        [HttpPut("{tenantId}/deactivate")]
        [ShouldHavePermission(SchoolAction.Update, SchoolFeature.Tenants)]
        public async Task<IActionResult> DeactivateTenantAsync([FromRoute] string tenantId)
        {
            var response = await Sender.Send(new DeactivateTenantCommand { TenantId = tenantId });
            if (!response.IsSuccessful)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        
        [HttpPut("{tenantId}/upgrade")]
        [ShouldHavePermission(SchoolAction.UpgradeSubscription, SchoolFeature.Tenants)]
        public async Task<IActionResult> UpgradeTenantSubscriptionAsync([FromRoute] string tenantId, [FromBody] UpdateTenantSubscriptionRequest updateTenant)
        {
            updateTenant.TenantId = tenantId;
            var response = await Sender.Send(new UpdateTenantSubscriptionCommand { UpdateTenantSubscription = updateTenant });
            if (!response.IsSuccessful)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("{tenantId}")]
        [ShouldHavePermission(SchoolAction.Read, SchoolFeature.Tenants)]
        public async Task<IActionResult> GetTenantById([FromRoute] string tenantId)
        {
            var response = await Sender.Send(new GetTenantByIdQuery { TenantId = tenantId });
            if (!response.IsSuccessful)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        
        [HttpGet("all")]
        [ShouldHavePermission(SchoolAction.Read, SchoolFeature.Tenants)]
        public async Task<IActionResult> GetTenantsAsync()
        {
            var response = await Sender.Send(new GetTenantsQuery());
            if (!response.IsSuccessful)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        
    }
}