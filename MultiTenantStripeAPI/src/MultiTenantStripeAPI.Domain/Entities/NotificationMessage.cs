using System.Text.Json.Serialization;

namespace MultiTenantStripeAPI.Domain.Entities
{
    public class NotificationMessage
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("recipientEmail")]
        public string RecipientEmail { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
