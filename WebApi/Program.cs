
using Application;
using Infrastructure;

namespace WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // __________________________________________________________________
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            // service 
            // builder.Services.AddEndpointsApiExplorer();
            // builder.Services.AddSwaggerGen();
            // __________________________________________________________________

            builder.Services.AddInfrastructureServices(builder.Configuration);

            // should add this 

            builder.Services.AddJwtAuthentication(builder.Services.GetJwtSettings(builder.Configuration));

            // from startup --. Application
            builder.Services.AddApplicationServices();
            
            
            var app = builder.Build();
            
            // Database Seeder 

            await app.Services.AddDatabaseInitializerAsync();

            // Configure the HTTP request pipeline.
            // Error handling middleware should be early in the pipeline
            app.UseMiddleware<ErrorHandlingMiddleware>();
            
            app.UseHttpsRedirection();
            
            // Add multi-tenancy and authentication/authorization middleware
            app.UseInfrastructure();
 
            app.MapControllers();

            app.Run();
        }
    }
}
