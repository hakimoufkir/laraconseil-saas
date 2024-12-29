using Microsoft.EntityFrameworkCore;
using Stripe;
using Consul;
using MultiTenantStripeAPI.Models;
using MultiTenantStripeAPI.Data;
using MultiTenantStripeAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Explicitly configure Kestrel to listen on the assigned port.
var port = builder.Configuration["SERVICE_PORT"] ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Stripe
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Configure DbContext for Tenant Management
builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Consul for service discovery
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(cfg =>
{
    var address = builder.Configuration["CONSUL_HTTP_ADDR"] ?? "http://localhost:8500";
    cfg.Address = new Uri(address);
}));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", policy =>
        policy
            .WithOrigins("http://localhost:4200") // Replace with your frontend URL
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // Allow cookies and credentials
});

// Build the app
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

app.UseCors("AllowOrigin");
app.UseHttpsRedirection();

// Tenant Middleware to validate tenant requests
app.UseMiddleware<TenantMiddleware>();

app.UseAuthorization();
app.MapControllers();

// Register service with Consul
var lifetime = app.Lifetime;
var consulClient = app.Services.GetRequiredService<IConsulClient>();
var registration = new AgentServiceRegistration
{
    ID = Guid.NewGuid().ToString(),
    Name = builder.Configuration["SERVICE_NAME"] ?? "multitenant-stripe-api",
    Address = builder.Configuration["SERVICE_HOST"] ?? "localhost",
    Port = int.Parse(builder.Configuration["SERVICE_PORT"] ?? "5187")
};

lifetime.ApplicationStarted.Register(() =>
{
    consulClient.Agent.ServiceRegister(registration).Wait();
    Console.WriteLine($"Service {registration.Name} registered with Consul at {registration.Address}:{registration.Port}");
});

lifetime.ApplicationStopped.Register(() =>
{
    consulClient.Agent.ServiceDeregister(registration.ID).Wait();
    Console.WriteLine($"Service {registration.Name} deregistered from Consul.");
});

app.Run();