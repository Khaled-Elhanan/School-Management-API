using Domain.Entities;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Tenacy;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context
{


    public class ApplicationDbContext : BaseDbContext
    {
        public ApplicationDbContext(IMultiTenantContextAccessor<ABCSchoolTenantInfo> tenantInfoContextAccessor, 
            DbContextOptions<ApplicationDbContext>options) :
            base(tenantInfoContextAccessor, options)
        {
          
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Ignore<ABCSchoolTenantInfo>();
        }
        public DbSet<School> Schools => Set<School>();
        
    }
}
