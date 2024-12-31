using Azure.Messaging.ServiceBus;

namespace MessagerService.Services
{
    public class ServiceBusListener : IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;

        public ServiceBusListener(ServiceBusClient client, IConfiguration configuration)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));

            string topicName = configuration["ServiceBus:TopicName"];
            string subscriptionName = configuration["ServiceBus:SubscriptionName"];

            _processor = _client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions());
        }

        public async Task StartAsync()
        {
            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;

            await _processor.StartProcessingAsync();
            Console.WriteLine("ServiceBusListener started successfully.");
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            try
            {
                string messageBody = args.Message.Body.ToString();
                Console.WriteLine($"Received message: {messageBody}");
                // Add your message processing logic here
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"Error on message processing: {args.Exception.Message}");
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await _processor.CloseAsync();
            await _client.DisposeAsync();
        }
    }
}
