namespace MultiTenantStripeAPI.Domain.Entities
{
    public class CreateCheckoutSessionRequest
    {
        public string TenantName { get; set; }
        public string Email { get; set; }
    }

}