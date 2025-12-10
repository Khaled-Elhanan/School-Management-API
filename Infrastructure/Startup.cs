using Application;
using Application.Features.identity.Tokens;
using Finbuckle.MultiTenant;
using Infrastructure.Constants;
using Infrastructure.Context;
using Infrastructure.Identity.Auth;
using Infrastructure.Identity.Models;
using Infrastructure.Identity.Tokens;
using Infrastructure.Tenacy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;
using System.Text;
using Application.Wrappers;
using Microsoft.AspNetCore.Http;
namespace Infrastructure
{
    public static class Startup
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
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

        public static async Task AddDatabaseInitializerAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();
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
                .Services
                .AddScoped<ITokenService, TokenService>();
        }


        internal static IServiceCollection AddPermissions(this IServiceCollection services)
        {
            return services
                .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
                .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        }
        // jwtsetting --> appsettings.json
        public static JwtSettings GetJwtSettings(this IServiceCollection services, IConfiguration config)
        {
            var jwtSettingsConfig = config.GetSection(nameof(JwtSettings));
            services.Configure<JwtSettings>(jwtSettingsConfig);
            return jwtSettingsConfig.Get<JwtSettings>();

        }
        // i want to read from json not from config here
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtSettings jwtSettings)
        {
            var secret = Encoding.ASCII.GetBytes(jwtSettings.Secret);
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(bearer =>
            {
                bearer.RequireHttpsMetadata = false;
                bearer.SaveToken = true;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {


                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))

                };
                bearer.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Response.ContentType = "application/json";
                            var result = JsonConvert.SerializeObject(ResponseWrapper.Fail("Token has expired."));
                            return context.Response.WriteAsync(result);
                        }
                        else
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            context.Response.ContentType = "application/json";
                            var result = JsonConvert.SerializeObject(ResponseWrapper.Fail("An unhandled error has occured"));
                            return context.Response.WriteAsync(result);
                        }
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Response.ContentType = "application/json";
                            var result = JsonConvert.SerializeObject(ResponseWrapper.Fail("You are not authorized."));
                            return context.Response.WriteAsync(result);
                        }
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        context.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(ResponseWrapper.Fail("You are not authorize to access this resource."));
                        return context.Response.WriteAsync(result);
                    }

                };

            });

           
        }

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
        {
            return app.UseMultiTenant();
        }
    }
}



