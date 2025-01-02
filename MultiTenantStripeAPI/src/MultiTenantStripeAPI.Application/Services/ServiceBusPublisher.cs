using Azure.Messaging.ServiceBus;
using MultiTenantStripeAPI.Application.IServices;

namespace MultiTenantStripeAPI.Application.Services
{
    public class ServiceBusPublisher : IServiceBusPublisher
    {
        private readonly ServiceBusClient _client;

        public ServiceBusPublisher(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

            _client = new ServiceBusClient(connectionString);
        }

        public async Task PublishMessageAsync(string topicName, string message)
        {
            if (string.IsNullOrEmpty(topicName)) throw new ArgumentException("Topic name cannot be null or empty.", nameof(topicName));
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            var sender = _client.CreateSender(topicName);

            try
            {
                var serviceBusMessage = new ServiceBusMessage(message);
                await sender.SendMessageAsync(serviceBusMessage);
                Console.WriteLine($"Message published to topic '{topicName}': {message}");
            }
            finally
            {
                await sender.DisposeAsync();
            }
        }
    }
}
