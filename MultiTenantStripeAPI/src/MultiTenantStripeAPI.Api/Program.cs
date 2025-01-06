using Stripe;
using MultiTenantStripeAPI.Api.Middleware;
using MultiTenantStripeAPI.Application;
using MultiTenantStripeAPI.Infrastructure;
using MultiTenantStripeAPI.Domain.Entities;
using MultiTenantStripeAPI.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Define required configuration keys for MultiTenant Service
var requiredKeysMultiTenant = new Dictionary<string, string>
{
    { "ConnectionStrings:AzureServiceBus", "The Azure Service Bus connection string is missing for MultiTenant Service." },
    { "ConnectionStrings:DefaultSQLConnection", "The SQL database connection string is missing for MultiTenant Service." },
    { "Stripe:SecretKey", "The Stripe secret key is missing for MultiTenant Service." },
    { "Stripe:PublishableKey", "The Stripe publishable key is missing for MultiTenant Service." },
    { "Stripe:WebhookSecret", "The Stripe webhook secret is missing for MultiTenant Service." },
    { "SERVICE_PORT", "The MultiTenant Service port is missing." }
};


// Verify all required keys for MultiTenant Service
foreach (var key in requiredKeysMultiTenant.Keys)
{
    var value = builder.Configuration[key];
    if (string.IsNullOrEmpty(value))
    {
        throw new ArgumentNullException(key, requiredKeysMultiTenant[key]);
    }
}

// Explicitly configure Kestrel to listen on the assigned port
var port = builder.Configuration["SERVICE_PORT"] ?? "5006";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Stripe
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));

var stripeSecretKey = builder.Configuration["Stripe:SecretKey"]
    ?? throw new InvalidOperationException("Stripe secret key is not set in the configuration.");
var stripeWebhookSecret = builder.Configuration["Stripe:WebhookSecret"]
    ?? throw new InvalidOperationException("Stripe webhook secret is not set in the configuration.");

StripeConfiguration.ApiKey = stripeSecretKey;



// Add Application and Infrastructure services
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", policy =>
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://subscription-app.gentlegrass-3889baac.westeurope.azurecontainerapps.io")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

// Apply migrations
// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     dbContext.Database.Migrate();
// }


// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowOrigin");
app.UseHttpsRedirection();

// Use custom middleware
app.UseMiddleware<TenantMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
