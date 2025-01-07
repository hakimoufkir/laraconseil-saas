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

        public async Task PublishMessageAsync(string topicName, object message)
        {
            var sender = _client.CreateSender(topicName);

            try
            {
                string messageBody = message is string ? message.ToString() : System.Text.Json.JsonSerializer.Serialize(message);
                var serviceBusMessage = new ServiceBusMessage(messageBody);
                await sender.SendMessageAsync(serviceBusMessage);
            }
            finally
            {
                await sender.DisposeAsync();
            }
        }
    }
}
