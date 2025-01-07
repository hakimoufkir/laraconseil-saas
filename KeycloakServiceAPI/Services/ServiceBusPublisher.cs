using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace KeycloakServiceAPI.Services;

public class ServiceBusPublisher
{
    private readonly ServiceBusClient _serviceBusClient;

    public ServiceBusPublisher(ServiceBusClient serviceBusClient)
    {
        _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
    }

    public async Task PublishNotificationAsync(string subject, string recipientEmail, string message)
    {
        // Construct the notification message
        var notification = new
        {
            Subject = subject,
            RecipientEmail = recipientEmail,
            Message = message
        };

        // Publish to the notifications topic
        var sender = _serviceBusClient.CreateSender("notifications");

        try
        {
            var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(notification));
            await sender.SendMessageAsync(serviceBusMessage);
            Console.WriteLine($"[Notifications] Message published: {notification}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Notifications] Error publishing message: {ex.Message}");
            throw;
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }
}
