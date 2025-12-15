
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

            var app = builder.Build();
            
            // Database Seeder 

            await app.Services.AddDatabaseInitializerAsync();

            // Configure the HTTP request pipeline.
            // if (app.Environment.IsDevelopment())
            // {
            //     app.UseSwagger();
            //     app.UseSwaggerUI();
            // }
            //
            // Add multi-tenancy middleware 
            app.UseInfrastructure();

            app.UseHttpsRedirection();
            
            // no need it after adding service AddJwtAuthentication
            // app.UseAuthorization();
 


            app.MapControllers();

            app.Run();
        }
    }
}
