using Microsoft.EntityFrameworkCore;
using Stripe;
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

// Configure In-Memory Database for Tenant Management
builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseInMemoryDatabase("MultiTenantDb"));

// Commented SQL Server configuration; retain for future use
/*
builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
*/

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", policy =>
        policy
            .WithOrigins("http://localhost:4200","https://subscription-app.gentlegrass-3889baac.westeurope.azurecontainerapps.io") // Replace with your frontend URL
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

// Seed data into the in-memory database
void SeedData(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

    // Example test tenants
    context.Tenants.Add(new Tenant
    {
        TenantId = Guid.NewGuid().ToString(),
        TenantName = "Test Tenant 1",
        Email = "tenant1@example.com",
        SubscriptionStatus = "Active"
    });

    context.Tenants.Add(new Tenant
    {
        TenantId = Guid.NewGuid().ToString(),
        TenantName = "Test Tenant 2",
        Email = "tenant2@example.com",
        SubscriptionStatus = "Pending"
    });

    context.SaveChanges();
}

// Call the seeding method to populate in-memory database
SeedData(app.Services);

app.Run();
