namespace MultiTenantStripeAPI.Domain.Entities
{
    public class NotificationPayload
    {
        public string Subject { get; set; }
        public string RecipientEmail { get; set; }
        public string TenantName { get; set; }
        public string Realm { get; set; }
        public string SubscriptionStatus { get; set; }
        public string Message { get; set; }
    }
}