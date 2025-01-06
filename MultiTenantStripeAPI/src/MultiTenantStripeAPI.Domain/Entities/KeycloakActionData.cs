namespace MultiTenantStripeAPI.Domain.Entities
{
    public class KeycloakActionData
    {
        public string RealmName { get; set; }
        public string TenantEmail { get; set; }
    }
}