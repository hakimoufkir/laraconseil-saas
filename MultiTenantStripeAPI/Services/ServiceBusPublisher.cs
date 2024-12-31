using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Services
{
    public class ServiceBusPublisher
    {
        private readonly string _connectionString;
        private readonly ServiceBusClient _client;

        public ServiceBusPublisher(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _client = new ServiceBusClient(_connectionString);
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
