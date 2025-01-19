namespace MultiTenantStripeAPI.Domain.Entities
{
    public class KeycloakActionData
    {
        public string? TenantName { get; set; }
        public string? TenantEmail { get; set; }
        public string? PlanType { get; set; }
    }
}