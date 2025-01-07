namespace MessagerService.Models
{
  public class NotificationMessage
    {
    public string Subject { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public NotificationPayload Payload { get; set; } = new NotificationPayload();
    }

}
