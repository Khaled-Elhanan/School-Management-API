using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Context;
using Infrastructure.Identity.Models;
using Infrastructure.Tenacy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Infrastructure.Constants
{
    public class ApplicationDbSeeder
    {
        private readonly IMultiTenantContextAccessor<ABCSchoolTenantInfo> _tenantContext;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ApplicationDbSeeder(
            RoleManager<ApplicationRole> roleManager,
            IMultiTenantContextAccessor<ABCSchoolTenantInfo> tenantContext,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _tenantContext = tenantContext;
            _userManager = userManager;
            _context = context;
        }

        // =========================================================
        // Entry
        // =========================================================
        public async Task InitalizeDatabaseAsync(CancellationToken cancellationToken)
        {
            if (!await _context.Database.CanConnectAsync(cancellationToken))
                return;

            var pending = await _context.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pending.Any())
                await _context.Database.MigrateAsync(cancellationToken);

            await InitializeDefaultRolesAsync(cancellationToken);
            await InitializeAdminUserAsync(cancellationToken);
        }

        // =========================================================
        // Roles (Tenant-Aware & Safe)
        // =========================================================
        private async Task InitializeDefaultRolesAsync(CancellationToken cancellationToken)
        {
            var tenant = _tenantContext.MultiTenantContext?.TenantInfo
                ?? throw new InvalidOperationException("Tenant context is missing.");

            var tenantId = tenant.Identifier!;

            foreach (var roleName in RoleConstants.DefaultRoles)
            {
                var tenantAwareRoleName = BuildTenantAwareName(tenantId, roleName);
                var role = await _roleManager.FindByNameAsync(tenantAwareRoleName);

                if (role == null)
                {
                    role = new ApplicationRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = tenantAwareRoleName,
                        NormalizedName = tenantAwareRoleName.ToUpperInvariant(),
                        Description = $"{roleName} Role",
                        TenantId = tenantId
                    };

                    var result = await _roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                        throw new Exception("Create role failed: " +
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                if (roleName == RoleConstants.Basic)
                {
                    await AssignPermissionsAsync(role, SchoolPermissions.Basic, cancellationToken);
                }
                else if (roleName == RoleConstants.Admin)
                {
                    await AssignPermissionsAsync(role, SchoolPermissions.Admin, cancellationToken);

                    if (tenantId == TenancyConstants.Root.Id)
                    {
                        await AssignPermissionsAsync(role, SchoolPermissions.Root, cancellationToken);
                    }
                }
            }
        }

        private async Task AssignPermissionsAsync(
            ApplicationRole role,
            IReadOnlyList<SchoolPermission> permissions,
            CancellationToken cancellationToken)
        {
            var existingClaims = await _roleManager.GetClaimsAsync(role);

            foreach (var permission in permissions)
            {
                if (existingClaims.Any(c =>
                        c.Type == ClaimConstats.Permissions &&
                        c.Value == permission.Name))
                    continue;

                await _context.RoleClaims.AddAsync(new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    ClaimType = ClaimConstats.Permissions,
                    ClaimValue = permission.Name,
                    Description = permission.Description
                }, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        // =========================================================
        // Admin User (Tenant-Aware & Bulletproof)
        // =========================================================
        private async Task InitializeAdminUserAsync(CancellationToken cancellationToken)
        {
            var tenant = _tenantContext.MultiTenantContext?.TenantInfo;
            if (tenant == null) return;

            var tenantId = tenant.Identifier!;
            var safeEmail = IsValidEmail(tenant.Email)
                ? tenant.Email!
                : $"{tenantId}@no-email.local";

            var tenantAwareUserName =
                Sanitize(BuildTenantAwareName(tenantId, safeEmail));

            var user = await _userManager.Users
                .SingleOrDefaultAsync(u =>
                    u.UserName == tenantAwareUserName &&
                    u.TenantId == tenantId,
                    cancellationToken);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = IsValidEmail(tenant.Email) ? tenant.Email : null,
                    UserName = tenantAwareUserName,
                    NormalizedEmail = IsValidEmail(tenant.Email)
                        ? tenant.Email!.ToUpperInvariant()
                        : null,
                    NormalizedUserName = tenantAwareUserName.ToUpperInvariant(),
                    FirstName = tenant.FirstName,
                    LastName = tenant.LastName,
                    IsActive = true,
                    TenantId = tenantId,
                    EmailConfirmed = IsValidEmail(tenant.Email)
                };

                var hasher = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = hasher.HashPassword(
                    user,
                    TenancyConstants.DefaultPaasword);

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                    throw new Exception("Create admin user failed: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            var adminRoleName = BuildTenantAwareName(tenantId, RoleConstants.Admin);

            if (!await _userManager.IsInRoleAsync(user, adminRoleName))
            {
                var result = await _userManager.AddToRoleAsync(user, adminRoleName);
                if (!result.Succeeded)
                    throw new Exception("Add admin role failed: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // =========================================================
        // Helpers
        // =========================================================
        private static string BuildTenantAwareName(string tenantId, string value)
            => $"{tenantId}__{value}";

        private static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try { _ = new MailAddress(email); return true; }
            catch { return false; }
        }

        private static string Sanitize(string input)
        {
            var lower = input.ToLowerInvariant();
            var clean = Regex.Replace(lower, "[^a-z0-9]", "_");
            clean = Regex.Replace(clean, "_{2,}", "_").Trim('_');
            return clean.Length > 200 ? clean[..200] : clean;
        }
    }
}
