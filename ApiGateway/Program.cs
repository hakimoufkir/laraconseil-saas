using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

namespace ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Explicitly configure Kestrel to listen on the assigned port.
            var port = builder.Configuration["SERVICE_PORT"] ?? "8080";
            builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

            // Add Ocelot and Consul configurations
            builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
            builder.Services.AddOcelot().AddConsul();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{builder.Configuration["SERVICE_NAME"]} API v1");
                });
            }

            app.UseAuthorization();

            // Configure Ocelot Middleware
            app.UseOcelot().Wait();

            app.Run();
        }
    }
}
