using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Context;
using Infrastructure.Identity.Models;
using Infrastructure.Tenacy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Constants
{
    public class ApplicationDbSeeder
    {
        public ApplicationDbSeeder(RoleManager<ApplicationRole> roleManager,
          IMultiTenantContextAccessor<ABCSchoolTenantInfo> tenantInfoContextAccessor,
          UserManager<ApplicationUser> userManager,
          ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _tenantInfoContextAccessor = tenantInfoContextAccessor;
            _userManager = userManager;
            _context = context;
        }
        private readonly IMultiTenantContextAccessor<ABCSchoolTenantInfo> _tenantInfoContextAccessor;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public async Task InitalizeDatabaseAsync(CancellationToken cancellationToken)
        {
            if (_context.Database.GetMigrations().Any())
            {
                if ((await _context.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
                {
                    await _context.Database.MigrateAsync(cancellationToken);
                }
                
                if (await _context.Database.CanConnectAsync(cancellationToken))
                {
                    
                    // Default Roles > Assgin permission/claims
                    
                    await InitializeDefaultRolesAsync(cancellationToken);
                    
                    // User > Assgin Roles 
                    await InitializeAdminUserAsync();




                }
            }
            
        }
    
        private async Task InitializeDefaultRolesAsync(CancellationToken cancellationToken)
        {
            // Assgin Role
            foreach (var roleName in RoleConstants.DefaultRoles)
            {
                var incomingRole = await _roleManager.Roles.SingleOrDefaultAsync(x => x.Name == roleName, cancellationToken);
                
                if (incomingRole is null)
                {
                    incomingRole = new ApplicationRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = roleName,
                        Description = $"{roleName} Role",
                        
                    };
                    await _roleManager.CreateAsync(incomingRole);
                }
                // Assgin permissions
                if (roleName == RoleConstants.Basic)
                {
                    await AssignPermissionToRole(SchoolPermissions.Basic , incomingRole ,cancellationToken);
                }
                else if (roleName == RoleConstants.Admin)
                {
                    await AssignPermissionToRole(SchoolPermissions.Admin, incomingRole, cancellationToken);
                    
                    if (_tenantInfoContextAccessor.MultiTenantContext?.TenantInfo?.Id == TenancyConstants.Root.Id)
                    {
                        await AssignPermissionToRole(SchoolPermissions.Root, incomingRole, cancellationToken);
                    }
                }

            }
        }
        private async Task AssignPermissionToRole(IReadOnlyList<SchoolPermission> rolePermission, 
            ApplicationRole role , CancellationToken cancellationToken)
        {
            var currentClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var permission in rolePermission)
            {
                if (!currentClaims.Any(x => x.Type == ClaimConstats.Permissions && x.Value == permission.Name))
                {
                    await _context.RoleClaims.AddAsync(new ApplicationRoleClaim
                    {
                        RoleId=role.Id,
                        RoleName=role.Name,
                        ClaimType=ClaimConstats.Permissions,
                        ClaimValue=permission.Name,
                        Description=permission.Description

                    }, cancellationToken);     
                    await _context.SaveChangesAsync(cancellationToken); 
                     
                }
            }
        }
        
        private async Task InitializeAdminUserAsync()
        {
            var tenantInfo = _tenantInfoContextAccessor.MultiTenantContext?.TenantInfo;
            if(tenantInfo == null || string.IsNullOrEmpty(tenantInfo.Email)) return;
            
            var incomingUser = await _userManager.Users.FirstOrDefaultAsync(user =>
                    user.Email == tenantInfo.Email);
            
            if (incomingUser is null)
            {
                incomingUser = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = tenantInfo.Email,
                    UserName = tenantInfo.Email,
                    FirstName = tenantInfo.FirstName,
                    LastName = tenantInfo.LastName,
                    PhoneNumberConfirmed = true ,
                    NormalizedEmail = tenantInfo.Email.ToUpperInvariant(),
                    NormalizedUserName = tenantInfo.Email.ToUpper(),
                    IsActive = true
                };
                var passwordHash = new PasswordHasher<ApplicationUser>();
                incomingUser.PasswordHash = passwordHash.
                    HashPassword(incomingUser, TenancyConstants.DefaultPaasword);
                await _userManager.CreateAsync(incomingUser);
                
            }

            if (!await _userManager.IsInRoleAsync(incomingUser, RoleConstants.Admin))
            {
                await _userManager.AddToRoleAsync(incomingUser, RoleConstants.Admin);
            }
            
            
        }

    }
}
