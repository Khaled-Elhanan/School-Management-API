using System.Security.Claims;
using Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Identity.Auth;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirment>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirment requirement)
    {
        var permissions = context.User.Claims
            .Where(x => x.Type == ClaimConstats.Permissions
                        && x.Value == requirement.Permission);
        if (permissions.Any())
        {
         context.Succeed(requirement); 
         await Task.CompletedTask;
        }
      
    }
    
}