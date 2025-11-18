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
                if (await _roleManager.Roles.SingleOrDefaultAsync(x => x.Name == roleName, cancellationToken) is not ApplicationRole incomingRole)
                {

                    incomingRole = new ApplicationRole
                    {
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
                    
                    if (_tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Id == TenancyConstants.Root.Id)
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
                        Description=permission.Description,
                        Group = permission.Group
                        

                    }, cancellationToken);     
                    await _context.SaveChangesAsync(cancellationToken); 
                     
                }
            }
        }
        

        private async Task InitializeAdminUserAsync()
        {
            if(string.IsNullOrEmpty(_tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email)) return;
            if (await _userManager.Users.FirstOrDefaultAsync(user =>
                    user.Email == _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email) is not ApplicationUser
                incomingUser)
            {
                incomingUser = new ApplicationUser
                {
                    Email = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email,
                    UserName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email,
                    FirstName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.FirstName,
                    LastName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.LastName,
                    PhoneNumberConfirmed = true ,
                    NormalizedEmail = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email.ToUpperInvariant(),
                    NormalizedUserName = _tenantInfoContextAccessor.MultiTenantContext.TenantInfo.Email.ToUpper(),
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
