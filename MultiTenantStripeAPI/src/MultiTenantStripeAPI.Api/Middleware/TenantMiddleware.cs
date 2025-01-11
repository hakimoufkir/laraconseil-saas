using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MultiTenantStripeAPI.Infrastructure.Persistence.Context;

namespace MultiTenantStripeAPI.Api.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        private static readonly List<string> ExcludedPaths = new()
        {
            "/api/Payment/webhook",
            "/api/Payment/create-checkout-session",
            "/api/Tenant/create",
            "/api/Role/assign-role",
            "/api/Role/check-permission",
            "/api/Cache/cache-test",
            "/api/Cache/all",
            "/api/Cache/set",
            "/api/Cache/{key}",
            "/api/Cache/",
            "/api/TestAuth/anonymous",
            "/api/TestAuth/grower",
            "/api/TestAuth/station-admin",
            "/api/TestAuth/claims"
        };

        public TenantMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var path = context.Request.Path.Value;

            // Bypass tenant validation for specific paths
            if (path != null && ExcludedPaths.Any(path.StartsWith))
            {
                Console.WriteLine($"Bypassing middleware for path: {path}");
                await _next(context);
                return;
            }

            var tenantId = context.Request.Headers["X-Tenant-ID"].FirstOrDefault();
            if (string.IsNullOrEmpty(tenantId))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Tenant ID is missing in the request headers.");
                return;
            }

            try
            {
                if (!_cache.TryGetValue(tenantId, out string? connectionString))
                {
                    var tenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.TenantId == tenantId);
                    if (tenant == null)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("Invalid Tenant");
                        return;
                    }

                    connectionString = tenant.DatabaseConnectionString;
                    // Add the entry to the cache with a size
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSize(1) // Specify the size of the cache entry
                        .SetSlidingExpiration(TimeSpan.FromMinutes(10));
                    _cache.Set(tenantId, connectionString, cacheEntryOptions);
                }

                // Inject tenant details into HttpContext.Items
                context.Items["TenantId"] = tenantId;
                context.Items["ConnectionString"] = connectionString;

                Console.WriteLine($"Tenant validation successful for TenantId: {tenantId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating tenant: {ex.Message}");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("An unexpected error occurred during tenant validation.");
                return;
            }

            await _next(context);
        }
    }
}
