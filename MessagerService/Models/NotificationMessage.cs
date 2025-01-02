namespace MessagerService.Models
{
    public class NotificationMessage
    {
        public string Subject { get; set; }
        public string RecipientEmail { get; set; }
        public dynamic Payload { get; set; } // Adjust type based on payload structure
    }

}
