using System.Linq;

namespace Infrastructure.Constants
{
    public static class SchoolAction
    {
        public const string Read = nameof(Read);
        public const string Create = nameof(Create);
        public const string Update = nameof(Update);
        public const string Delete = nameof(Delete);
        public const string RefreshToken = nameof(RefreshToken);
        public const string UpgradeSubscription = nameof(UpgradeSubscription);
    }

    public static class SchoolFeature
    {
        public const string Tenants = nameof(Tenants);
        public const string Users = nameof(Users);
        public const string Roles = nameof(Roles);
        public const string UserRoles = nameof(UserRoles);
        public const string RoleClaims = nameof(RoleClaims);
        public const string Schools = nameof(Schools);
        public const string Tokens = nameof(Tokens);
    }

    public record SchoolPermission(
      string Action,
      string Feature,
      string Description,
      string Group = "",
      bool IsBasic = false,
      bool IsRoot = false)
    {
        public string Name => NameFor(Action, Feature);

        public static string NameFor(string action, string feature)
            => $"Permission.{feature}.{action}";
    }


    public static class SchoolPermissions
    {
        private static readonly SchoolPermission[] _allPermissions =
        {
            // ================= ROOT =================
            new SchoolPermission(
                SchoolAction.Create,
                SchoolFeature.Tenants,
                "Permission to create tenants",
                "Tenancy",
                IsRoot: true),

            new SchoolPermission(
                SchoolAction.Read,
                SchoolFeature.Tenants,
                "Permission to read tenants",
                "Tenancy",
                IsRoot: true),

            new SchoolPermission(
                SchoolAction.Delete,
                SchoolFeature.Tenants,
                "Permission to delete tenants",
                "Tenancy",
                IsRoot: true),

            // ================= USERS =================
            new SchoolPermission(
                SchoolAction.Read,
                SchoolFeature.Users,
                "Permission to read users",
                "SystemAccess",
                IsBasic: true),

            new SchoolPermission(
                SchoolAction.Create,
                SchoolFeature.Users,
                "Permission to create users",
                "SystemAccess"),

            new SchoolPermission(
                SchoolAction.Update,
                SchoolFeature.Users,
                "Permission to update users",
                "SystemAccess"),

            new SchoolPermission(
                SchoolAction.Delete,
                SchoolFeature.Users,
                "Permission to delete users",
                "SystemAccess"),

            // ================= USER ROLES =================
            new SchoolPermission(
                SchoolAction.Read,
                SchoolFeature.UserRoles,
                "Permission to read user roles",
                "SystemAccess",
                IsBasic: true),

            new SchoolPermission(
                SchoolAction.Create,
                SchoolFeature.UserRoles,
                "Permission to create user roles",
                "SystemAccess"),

            new SchoolPermission(
                SchoolAction.Update,
                SchoolFeature.UserRoles,
                "Permission to update user roles",
                "SystemAccess"),

            new SchoolPermission(
                SchoolAction.Delete,
                SchoolFeature.UserRoles,
                "Permission to delete user roles",
                "SystemAccess"),

            // ================= ROLES =================
            new SchoolPermission(
                SchoolAction.Read,
                SchoolFeature.Roles,
                "Permission to read roles",
                "SystemAccess",
                IsBasic: true),

            new SchoolPermission(
                SchoolAction.Create,
                SchoolFeature.Roles,
                "Permission to create roles",
                "SystemAccess"),

            new SchoolPermission(
                SchoolAction.Update,
                SchoolFeature.Roles,
                "Permission to update roles",
                "SystemAccess"),

            new SchoolPermission(
                SchoolAction.Delete,
                SchoolFeature.Roles,
                "Permission to delete roles",
                "SystemAccess"),

            // ================= ROLE CLAIMS =================
            new SchoolPermission(
                SchoolAction.Read,
                SchoolFeature.RoleClaims,
                "Read Role Claims/Permissions",
                "SystemAccess"),

            new SchoolPermission(
                SchoolAction.Update,
                SchoolFeature.RoleClaims,
                "Update Role Claims/Permissions",
                "SystemAccess"),

            // ================= SCHOOLS (TENANT) =================
            new SchoolPermission(
                SchoolAction.Read,
                SchoolFeature.Schools,
                "Permission to read schools",
                "Academics",
                IsBasic: true),

            new SchoolPermission(
                SchoolAction.Create,
                SchoolFeature.Schools,
                "Permission to create schools",
                "Academics"),

            new SchoolPermission(
                SchoolAction.Update,
                SchoolFeature.Schools,
                "Permission to update schools",
                "Academics"),

             new SchoolPermission(
                SchoolAction.Delete,
                SchoolFeature.Schools,
                "Permission to delete schools",
                "Academics"),

            new SchoolPermission(
                SchoolAction.UpgradeSubscription,
                SchoolFeature.Schools,
                "Permission to upgrade school subscription",
                "Academics"),

            // ================= TOKENS =================
            new SchoolPermission(
                SchoolAction.RefreshToken,
                SchoolFeature.Tokens,
                "Generate Refresh Token",
                "SystemAccess",
                IsBasic: true),
        };

        public static IReadOnlyList<SchoolPermission> All => _allPermissions;
        public static IReadOnlyList<SchoolPermission> Root => _allPermissions.Where(p => p.IsRoot).ToArray();
        public static IReadOnlyList<SchoolPermission> Admin => _allPermissions.Where(p => !p.IsRoot).ToArray();
        public static IReadOnlyList<SchoolPermission> Basic => _allPermissions.Where(p => p.IsBasic).ToArray();
    }
}
