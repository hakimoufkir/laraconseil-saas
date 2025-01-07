namespace MultiTenantStripeAPI.Domain.Entities
{
    public class KeycloakActionMessage
    {
        public string? Action { get; set; }
        public KeycloakActionData? Data { get; set; }
    }
}