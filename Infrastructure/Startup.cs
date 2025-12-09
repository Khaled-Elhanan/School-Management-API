using Finbuckle.MultiTenant;
using Infrastructure.Constants;
using Infrastructure.Context;
using Infrastructure.Identity.Auth;
using Infrastructure.Identity.Models;
using Infrastructure.Tenacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Infrastructure
{
    public static class Startup
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services ,
            IConfiguration config)
        {
            return services
                .AddDbContext<TenantDbContext>(options =>
                    options.UseSqlServer(config.GetConnectionString("DefaultConnection")))

                .AddMultiTenant<ABCSchoolTenantInfo>()
                .WithHeaderStrategy(TenancyConstants.TenantIdName)
                .WithClaimStrategy(TenancyConstants.TenantIdName)
                .WithEFCoreStore<TenantDbContext, ABCSchoolTenantInfo>()
                .Services

                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(config.GetConnectionString("DefaultConnection")))
                .AddTransient<ITenantDbSeeder, TenantDbSeeder>()
                .AddTransient<ApplicationDbSeeder>()
                .AddIdentityServices()
                .AddPermissions();

        }

        public static async Task AddDatabaseInitializerAsync(this IServiceProvider  serviceProvider , CancellationToken cancellationToken=default)
        {
            using var scope =serviceProvider.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ITenantDbSeeder>()
                .IntializeDatabaseAsync(cancellationToken);
        }

        internal static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            return services
                .AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                }).AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .Services;
        }


        internal static IServiceCollection AddPermissions(this IServiceCollection services)
        {
            return services
                .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
                .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        }
        
        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
        {
            return app.UseMultiTenant();
        }
    }
}

     

