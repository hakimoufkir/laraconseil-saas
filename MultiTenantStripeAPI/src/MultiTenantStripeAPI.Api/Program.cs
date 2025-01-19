using Stripe;
using MultiTenantStripeAPI.Api.Middleware;
using MultiTenantStripeAPI.Application;
using MultiTenantStripeAPI.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using MultiTenantStripeAPI.Api.Extensions;
using MultiTenantStripeAPI.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);


// Explicitly configure Kestrel to listen on the assigned port
var port = builder.Configuration["MULTITENANT_API_PORT"] ?? "5005";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Configuration validation
var requiredKeysMultiTenant = new Dictionary<string, string>
{
    { "ConnectionStrings:AzureServiceBus", "Azure Service Bus connection string is missing." },
    
    { "ConnectionStrings:DefaultSQLConnection", "Postgresql connection string is missing." },
    
    { "Stripe:SecretKey", "Stripe secret key is missing." },
    { "Stripe:PublishableKey", "Stripe publishable key is missing." },
    { "Stripe:WebhookSecret", "Stripe webhook secret is missing." },

    { "Authentication:Audience", "Authentication audience is missing." },
    { "Authentication:MetadataAddress", "Authentication metadata address is missing." },
    { "Authentication:ValidIssuer", "Authentication valid issuer is missing." },
    { "Keycloak:AuthorizationUrl", "Keycloak authorization URL is missing."}
};

// Validate configuration
foreach (var key in requiredKeysMultiTenant.Keys)
{
    var value = builder.Configuration[key];
    if (string.IsNullOrEmpty(value))
    {
        Console.WriteLine($"[ERROR] Missing configuration: {key}");
        throw new ArgumentNullException(key, requiredKeysMultiTenant[key]);
    }
}

Console.WriteLine("[INFO] Configuration validated successfully.");

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth(builder.Configuration);
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Cache size limit
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
});

// Stripe configuration
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
Console.WriteLine("[INFO] Stripe API Key configured.");

// Add application and infrastructure services
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
Console.WriteLine("[INFO] Application and infrastructure services added.");

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.MetadataAddress = builder.Configuration["Authentication:MetadataAddress"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Authentication:ValidIssuer"]
        };
    });
Console.WriteLine("[INFO] Authentication configured.");

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    // GrowerPolicy
    options.AddPolicy("GrowerPolicy", policy =>
    {
        policy.RequireAssertion(context =>
        {
            Console.WriteLine("[INFO] Evaluating GrowerPolicy...");
            var roleClaim = context.User.Claims.FirstOrDefault(c => c.Type == "resource_access");

            Console.WriteLine($"[DEBUG] Full JWT Claims: {string.Join(", ", context.User.Claims.Select(c => $"{c.Type}: {c.Value}"))}");


            if (roleClaim == null)
            {
                Console.WriteLine("[WARNING] Role claim not found in user token.");
                return false;
            }

            var resourceAccess = System.Text.Json.JsonDocument.Parse(roleClaim.Value);
            if (resourceAccess.RootElement.TryGetProperty("LaraConseil", out var clientRoles) &&
                clientRoles.TryGetProperty("roles", out var roles))
            {
                var hasRole = roles.EnumerateArray().Any(r => r.GetString() == "Grower");
                Console.WriteLine($"[INFO] Grower role check: {hasRole}");
                return hasRole;
            }

            Console.WriteLine("[WARNING] Grower role not found in resource_access.");
            return false;
        });
    });


    // StationPolicy
    options.AddPolicy("StationPolicy", policy =>
    {
        policy.RequireAssertion(context =>
        {
            Console.WriteLine("[INFO] Evaluating StationPolicy...");
            var roleClaim = context.User.Claims.FirstOrDefault(c => c.Type == "resource_access");

            if (roleClaim == null)
            {
                Console.WriteLine("[WARNING] Role claim not found in user token.");
                return false;
            }

            Console.WriteLine($"[DEBUG] Role Claim Value: {roleClaim.Value}");

            try
            {
                var resourceAccess = System.Text.Json.JsonDocument.Parse(roleClaim.Value);

                if (!resourceAccess.RootElement.TryGetProperty("LaraConseil", out var clientRoles))
                {
                    Console.WriteLine("[WARNING] LaraConseil property not found in resource_access.");
                    return false;
                }

                if (!clientRoles.TryGetProperty("roles", out var roles))
                {
                    Console.WriteLine("[WARNING] Roles property not found in LaraConseil.");
                    return false;
                }

                var hasRole = roles.EnumerateArray().Any(r => r.GetString() == "Station");
                Console.WriteLine($"[INFO] Station role check: {hasRole}");
                return hasRole;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception during StationPolicy evaluation: {ex.Message}");
                return false;
            }
        });
    });



});
Console.WriteLine("[INFO] Authorization policies configured.");

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", policy =>
        policy.WithOrigins("http://localhost:4200", "https://webapp.gentlegrass-3889baac.westeurope.azurecontainerapps.io")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
Console.WriteLine("[INFO] CORS policy configured.");

var app = builder.Build();

Console.WriteLine("[INFO] Application has started.");


// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    Console.WriteLine("[INFO] Swagger UI enabled.");
}

app.UseCors("AllowOrigin");
app.UseHttpsRedirection();
app.UseMiddleware<TenantMiddleware>();
Console.WriteLine("[INFO] TenantMiddleware added to pipeline.");

app.UseAuthentication();
Console.WriteLine("[INFO] Authentication middleware added to pipeline.");

app.UseAuthorization();
Console.WriteLine("[INFO] Authorization middleware added to pipeline.");

app.MapControllers();

app.Run();
