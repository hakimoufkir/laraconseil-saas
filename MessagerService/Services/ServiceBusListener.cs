using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using MessagerService.Models;

namespace MessagerService.Services
{
    public class ServiceBusListener : IAsyncDisposable
    {
        private readonly ServiceBusProcessor _notificationsProcessor;
        private readonly ServiceBusProcessor _subscriptionEventsProcessor;
        private readonly EmailService _emailService;

        public ServiceBusListener(ServiceBusClient client, IConfiguration configuration, EmailService emailService)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));

            // Notifications topic and subscription
            _notificationsProcessor = client.CreateProcessor("notifications", "messager-service-sub", new ServiceBusProcessorOptions());

            // Subscription-events topic and subscription
            _subscriptionEventsProcessor = client.CreateProcessor("subscription-events", "messager-service-sub", new ServiceBusProcessorOptions());
        }

        public async Task StartAsync()
        {
            // Notifications topic processor
            _notificationsProcessor.ProcessMessageAsync += NotificationsMessageHandler;
            _notificationsProcessor.ProcessErrorAsync += ErrorHandler;
            await _notificationsProcessor.StartProcessingAsync();
            Console.WriteLine("Listening to 'notifications' with subscription 'messager-service-sub'...");

            // Subscription-events topic processor
            _subscriptionEventsProcessor.ProcessMessageAsync += SubscriptionEventsMessageHandler;
            _subscriptionEventsProcessor.ProcessErrorAsync += ErrorHandler;
            await _subscriptionEventsProcessor.StartProcessingAsync();
            Console.WriteLine("Listening to 'subscription-events' with subscription 'messager-service-sub'...");
        }

        private async Task NotificationsMessageHandler(ProcessMessageEventArgs args)
        {
            try
            {
                string messageBody = args.Message.Body.ToString();
                Console.WriteLine($"[Notifications] Received message: {messageBody}");

                var notification = JsonConvert.DeserializeObject<NotificationMessage>(messageBody);
                if (notification == null || string.IsNullOrEmpty(notification.Subject) || string.IsNullOrEmpty(notification.RecipientEmail))
                {
                    Console.WriteLine("[Notifications] Invalid message payload.");
                    return;
                }

                // Handle specific notifications based on the subject
                switch (notification.Subject)
                {
                    case "Realm Created":
                        await HandleRealmCreated(notification);
                        break;

                    default:
                        Console.WriteLine($"[Notifications] Unhandled notification subject: {notification.Subject}");
                        break;
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Notifications] Error processing message: {ex.Message}");
            }
        }

        private async Task SubscriptionEventsMessageHandler(ProcessMessageEventArgs args)
        {
            try
            {
                string messageBody = args.Message.Body.ToString();
                Console.WriteLine($"[SubscriptionEvents] Received message: {messageBody}");

                var subscriptionEvent = JsonConvert.DeserializeObject<NotificationMessage>(messageBody);
                if (subscriptionEvent == null || string.IsNullOrEmpty(subscriptionEvent.Subject) || string.IsNullOrEmpty(subscriptionEvent.RecipientEmail))
                {
                    Console.WriteLine("[SubscriptionEvents] Invalid message payload.");
                    return;
                }

                // Handle subscription-related events
                switch (subscriptionEvent.Subject)
                {
                    case "Subscription Activated":
                    case "Subscription Updated":
                    case "Subscription Canceled":
                        await HandleSubscriptionUpdatedOrCanceled(subscriptionEvent);
                        break;

                    default:
                        Console.WriteLine($"[SubscriptionEvents] Unhandled subscription event subject: {subscriptionEvent.Subject}");
                        break;
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubscriptionEvents] Error processing message: {ex.Message}");
            }
        }
        private async Task HandleRealmCreated(NotificationMessage notification)
        {
            Console.WriteLine($"Processing realm creation for: {notification?.RecipientEmail ?? "undefined"}");
            string subject = "Your Account Has Been Created";
            string body = $@"
        <h1>Welcome to Our Platform!</h1>
        <p>Your account has been created under the realm: <strong>{notification?.Payload?.Realm ?? "undefined"}</strong>.</p>
        <p>Details: {notification?.Payload?.Message ?? "No additional details available."}</p>";

            await _emailService.SendEmailAsync(notification?.RecipientEmail ?? "undefined@domain.com", subject, body);
        }

        private async Task HandleSubscriptionUpdatedOrCanceled(NotificationMessage notification)
        {
            Console.WriteLine($"Processing subscription update/cancellation for: {notification?.RecipientEmail ?? "undefined"}");
            string subject = $"Your Subscription Has Been {notification?.Payload?.SubscriptionStatus ?? "undefined"}!";
            string body = $@"
        <h1>Hello, {notification?.Payload?.TenantName ?? "User"}!</h1>
        <p>{notification?.Payload?.Message ?? "No additional details available."}</p>";

            await _emailService.SendEmailAsync(notification?.RecipientEmail ?? "undefined@domain.com", subject, body);
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"Error on message processing: {args.Exception.Message}");
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await _notificationsProcessor.CloseAsync();
            await _subscriptionEventsProcessor.CloseAsync();
        }
    }
}
