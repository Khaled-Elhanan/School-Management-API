using Domain.Entities;
using Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Context
{
    internal class DbConfigurations
    {
        internal class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
        {
            public void Configure(EntityTypeBuilder<ApplicationUser> builder)
            {
                builder
                .ToTable("Users", "Identity");
                // Multi-tenancy is automatically configured by MultiTenantIdentityDbContext
            }

        }
        internal class ApplicationRoleConfig : IEntityTypeConfiguration<ApplicationRole>
        {
            public void Configure(EntityTypeBuilder<ApplicationRole> builder)
            {
                builder
                .ToTable("Roles", "Identity");
               
            }
        }
        internal class ApplicationRoleClaimConfig : IEntityTypeConfiguration<ApplicationRoleClaim>
        {
            public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
            {
                builder
                .ToTable("RoleClaims", "Identity");
                
            }
        }
        internal class ApplicationUserRoleConfig : IEntityTypeConfiguration<IdentityUserRole<string>>
        {
            public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
            {
                builder
                .ToTable("UserRoles", "Identity");
              
            }
        }

        internal class ApplicationUserClaimConfig : IEntityTypeConfiguration<IdentityUserClaim<string>>
        {
            public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder)
            {
                builder
                .ToTable("UserClaims", "Identity");
             
            }
        }
        internal class ApplicationUserLoginConfig : IEntityTypeConfiguration<IdentityUserLogin<string>>
        {
            public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder)
            {
                builder
                .ToTable("UserLogins", "Identity");
            }
        }
        internal class ApplicationUserTokenConfig : IEntityTypeConfiguration<IdentityUserToken<string>>
        {
            public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder)
            {
                builder
                .ToTable("UserTokens", "Identity");
            }
        }
        internal class  SchoolConfig : IEntityTypeConfiguration<School>
        {
            public void Configure(EntityTypeBuilder<School> builder)
            {
                builder
                .ToTable("Schools" , "Academics");

                builder.Property(s => s.Name)
                    .IsRequired()
                    .HasMaxLength(60);
            }   
        }
    }
}
