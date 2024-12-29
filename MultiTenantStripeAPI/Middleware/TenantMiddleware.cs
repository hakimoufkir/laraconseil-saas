using MultiTenantStripeAPI.Data;

namespace MultiTenantStripeAPI.Middleware
{

    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, TenantDbContext dbContext)
        {
            var path = context.Request.Path.Value;

            // Bypass tenant validation for specific paths == Exclude certain paths from tenant validation
            if (path != null && (path.StartsWith("/api/Payment/webhook") || path.StartsWith("/api/Payment/create-checkout-session") || path.StartsWith("/api/Debug")))
            {
                await _next(context);
                return;
            }         

            // Tenant validation for other endpoints
            var tenantId = context.Request.Headers["X-Tenant-ID"].FirstOrDefault();

            if (string.IsNullOrEmpty(tenantId) || !dbContext.Tenants.Any(t => t.TenantId == tenantId))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Invalid Tenant");
                return;
            }

            await _next(context);
        }

    }

}
