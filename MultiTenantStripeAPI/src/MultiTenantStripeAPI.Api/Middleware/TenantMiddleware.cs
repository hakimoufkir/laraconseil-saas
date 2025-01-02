using Microsoft.EntityFrameworkCore;
using MultiTenantStripeAPI.Infrastructure.Persistence.Context;

namespace MultiTenantStripeAPI.Api.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var path = context.Request.Path.Value;

            // Bypass tenant validation for specific paths (Excluded paths)
            if (path != null &&
                (path.StartsWith("/api/Payment/webhook") ||
                 path.StartsWith("/api/Payment/create-checkout-session") ||
                 path.StartsWith("/api/Debug")))
            {
                Console.WriteLine($"Bypassing middleware for path: {path}");
                await _next(context);
                return;
            }

            // Extract Tenant ID from headers
            var tenantId = context.Request.Headers["X-Tenant-ID"].FirstOrDefault();

            if (string.IsNullOrEmpty(tenantId))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Tenant ID is missing in the request headers.");
                return;
            }

            // Validate Tenant ID exists in the database
            var tenantExists = await dbContext.Tenants.AnyAsync(t => t.TenantId == tenantId);

            if (!tenantExists)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Invalid Tenant");
                return;
            }

            await _next(context);
        }
    }
}
