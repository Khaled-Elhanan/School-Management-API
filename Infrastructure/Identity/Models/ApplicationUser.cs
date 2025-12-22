using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity.Models
{
    public  class ApplicationUser : IdentityUser<string>
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public bool IsActive { get; set; }
        // Initialize RefreshToken to empty string to avoid null inserts into a non-nullable column
        public string  RefreshToken { get; set; } = string.Empty;

        public DateTime RefreshTokenExpiryTime   { get; set; }
        public string? TenantId { get; set; }

    }
}
