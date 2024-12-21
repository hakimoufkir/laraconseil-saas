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

            // Add Ocelot and Consul configurations
            builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
            builder.Services.AddOcelot().AddConsul();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure Kestrel
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5130); // HTTP
                // options.ListenAnyIP(5001, listenOptions =>
                // {
                //     listenOptions.UseHttps(
                //         Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path") ?? "/https/https-dev-cert.pfx",
                //         Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password") ?? "Test@123"
                //     ); // HTTPS
                // });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection(); // Redirect HTTP to HTTPS (optional, comment if not needed)

            app.UseAuthorization();

            // Configure Ocelot Middleware
            app.UseOcelot().Wait();

            app.Run();
        }
    }
}
