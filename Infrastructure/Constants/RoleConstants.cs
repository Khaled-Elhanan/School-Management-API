namespace Infrastructure.Constants
{
    public static class RoleConstants
    {
        public const string Admin = nameof(Admin);
        public const string Basic = nameof(Basic);
        public static IReadOnlyList<string> DefaultRoles { get; } = new List<string>
            {
                Admin,
                Basic
            }.AsReadOnly();

        public static bool IsDefaultRole(string roleName) => DefaultRoles.Contains(roleName);

        
      

    }
}
