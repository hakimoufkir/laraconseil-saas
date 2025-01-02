namespace MultiTenantStripeAPI.Domain.Entities
{
    public class Tenant
    {
        public int Id { get; set; }
        public string? TenantName { get; set; }
        public string? TenantId { get; set; }
        public string? Email { get; set; }
        public string? SubscriptionStatus { get; set; } // Active, Expired
    }
}