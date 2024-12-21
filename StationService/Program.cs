using Consul;

namespace StationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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

            // Configure Kestrel
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5004); // HTTP
                // options.ListenAnyIP(5005, listenOptions =>
                // {
                //     listenOptions.UseHttps(
                //         Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path") ?? "/https/https-dev-cert.pfx",
                //         Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password") ?? "Test@123"
                //     ); // HTTPS
                // });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();

            // Register service with Consul
            var lifetime = app.Lifetime;
            var consulClient = app.Services.GetRequiredService<IConsulClient>();
            var registration = new AgentServiceRegistration
            {
                ID = Guid.NewGuid().ToString(),
                Name = "station-service",
                Address = "station-service",
                Port = 5004 // HTTP port
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
