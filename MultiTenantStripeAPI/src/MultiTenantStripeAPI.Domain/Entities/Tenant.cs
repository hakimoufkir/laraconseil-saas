namespace MultiTenantStripeAPI.Domain.Entities
{
    public class Tenant
    {
        public string TenantId { get; set; } // Primary key
        public string TenantName { get; set; }
        public string Email { get; set; }
        public string SubscriptionStatus { get; set; }
        public string PlanType { get; set; }
        public string DatabaseConnectionString { get; set; }
        public int MaxUsers { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? BrandingLogoUrl { get; set; }
        public string? ThemeColor { get; set; }
        public string? DeactivationReason { get; set; }
    }
}
