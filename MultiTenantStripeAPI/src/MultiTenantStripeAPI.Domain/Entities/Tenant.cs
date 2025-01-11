namespace MultiTenantStripeAPI.Domain.Entities
{
    public class Tenant
    {
        public string? TenantId { get; set; }
        public string? TenantName { get; set; }
        public string? Email { get; set; }
        public string? SubscriptionStatus { get; set; } // Active, Expired
        public string? DatabaseConnectionString { get; set; } // Connection string for tenant-specific DB
    }
}