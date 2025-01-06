using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace KeycloakServiceAPI.Services;

public class ServiceBusPublisher
{
    private readonly ServiceBusClient _serviceBusClient;

    public ServiceBusPublisher(ServiceBusClient serviceBusClient)
    {
        _serviceBusClient = serviceBusClient;
    }

    public async Task PublishMessageAsync(string topicName, object message)
    {
        var sender = _serviceBusClient.CreateSender(topicName);

        try
        {
            var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(message));
            await sender.SendMessageAsync(serviceBusMessage);
            Console.WriteLine($"Message published to topic '{topicName}': {message}");
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }
}
