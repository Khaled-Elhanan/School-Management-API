using System.Security.Claims;
using Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;

using Infrastructure.Tenacy;

namespace Infrastructure.Identity.Auth;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirment>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirment requirement)
    {
        var permissionDef = SchoolPermissions.All.FirstOrDefault(p => p.Name == requirement.Permission);

        if (permissionDef != null && permissionDef.IsRoot)
        {
            var tenantId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimConstats.Tenant)?.Value;
            if (tenantId != TenancyConstants.Root.Id)
            {
                return Task.CompletedTask;
            }
        }

        var permissions = context.User.Claims
            .Where(x => x.Type == ClaimConstats.Permissions
                        && x.Value == requirement.Permission);

        if (permissions.Any())
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
    
}