using Azure.Messaging.ServiceBus;
using MessagerService.Models;
using Newtonsoft.Json;

namespace MessagerService.Services
{
    public class ServiceBusListener : IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;
        private readonly EmailService _emailService;

        public ServiceBusListener(ServiceBusClient client, IConfiguration configuration, EmailService emailService)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));

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

                var notification = JsonConvert.DeserializeObject<NotificationMessage>(messageBody);
                Console.WriteLine($"Deserialized Notification: {notification}");

                if (notification == null || string.IsNullOrEmpty(notification.Subject) || string.IsNullOrEmpty(notification.RecipientEmail))
                {
                    Console.WriteLine("Invalid notification message payload.");
                    return;
                }

                // Handle specific notifications based on the subject
                switch (notification.Subject)
                {
                    case "Subscription Activated":
                        await HandleSubscriptionActivated(notification);
                        break;

                    case "Realm Created":
                        await HandleRealmCreated(notification);
                        break;

                    default:
                        Console.WriteLine($"Unhandled notification subject: {notification.Subject}");
                        break;
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }

        private async Task HandleSubscriptionActivated(NotificationMessage notification)
        {
            Console.WriteLine($"Processing subscription activation for: {notification.RecipientEmail}");
            string subject = "Your Subscription Has Been Activated!";
            string body = $@"
                <h1>Welcome, {notification.Payload.TenantName}!</h1>
                <p>{notification.Payload.Message}</p>";

            await _emailService.SendEmailAsync(notification.RecipientEmail, subject, body);
        }

        private async Task HandleRealmCreated(NotificationMessage notification)
        {
            Console.WriteLine($"Processing realm creation for: {notification.RecipientEmail}");
            string subject = "Your Account Has Been Created";
            string body = $@"
                <h1>Welcome to Our Platform!</h1>
                <p>Your account has been created under the realm: <strong>{notification.Payload.TenantName}</strong>.</p>
                <p>Details: {notification.Payload.Message}</p>";

            await _emailService.SendEmailAsync(notification.RecipientEmail, subject, body);
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
