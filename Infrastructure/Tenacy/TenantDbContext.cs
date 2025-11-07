using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tenacy
{
    public class TenantDbContext(DbContextOptions<TenantDbContext> options):
        EFCoreStoreDbContext<ABCSchoolTenantInfo> (options) 
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ABCSchoolTenantInfo>()
                .ToTable("Tenants","MultiTenancy");
        }


    }
}
