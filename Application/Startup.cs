using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Application;

public static class Startup
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        return services
            .AddValidatorsFromAssembly(assembly)
            .AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
            });

    }
}
