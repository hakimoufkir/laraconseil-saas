using Consul;

namespace GrowerService
{
    public class Program
    {
         public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Explicitly configure Kestrel to listen on the assigned port.
            var port = builder.Configuration["SERVICE_PORT"] ?? "8080";
            builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure Consul for service discovery
            builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(cfg =>
            {
                var address = builder.Configuration["CONSUL_HTTP_ADDR"] ?? "http://localhost:8500";
                cfg.Address = new Uri(address);
            }));

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
            app.MapControllers();

            // Register service with Consul
            var lifetime = app.Lifetime;
            var consulClient = app.Services.GetRequiredService<IConsulClient>();
            var registration = new AgentServiceRegistration
            {
                ID = Guid.NewGuid().ToString(),
                Name = builder.Configuration["SERVICE_NAME"] ?? "default-service",
                Address = builder.Configuration["SERVICE_HOST"] ?? "localhost",
                Port = int.Parse(builder.Configuration["SERVICE_PORT"] ?? "5000")
            };

            lifetime.ApplicationStarted.Register(() =>
            {
                consulClient.Agent.ServiceRegister(registration).Wait();
            });

            lifetime.ApplicationStopped.Register(() =>
            {
                consulClient.Agent.ServiceDeregister(registration.ID).Wait();
            });

            app.Run();
        }
    }
}
