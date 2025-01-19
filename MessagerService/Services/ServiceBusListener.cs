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
                    await args.DeadLetterMessageAsync(args.Message);
                    return;
                }

                // Handle specific notifications based on the subject
                switch (notification.Subject)
                {
                    case "User Created":
                        await HandleUserCreated(notification);
                        break;

                    case "Roles Assigned to User":
                        await HandleRolesAssigned(notification);
                        break;

                    default:
                        Console.WriteLine($"[Notifications] Unhandled notification subject: {notification.Subject}");
                        await args.DeadLetterMessageAsync(args.Message); // Dead-letter if action is not recognized
                        break;
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Notifications] Error processing message: {ex.Message}");
                await args.DeadLetterMessageAsync(args.Message); // Dead-letter on error
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
                    await args.DeadLetterMessageAsync(args.Message);
                    return;
                }

                // Handle subscription-related events
                switch (subscriptionEvent.Subject)
                {
                    case "Subscription Activated":
                        await HandleSubscriptionActivated(subscriptionEvent);
                        break;

                    case "Subscription Updated":
                        await HandleSubscriptionUpdated(subscriptionEvent);
                        break;

                    case "Subscription Canceled":
                        await HandleSubscriptionCanceled(subscriptionEvent);
                        break;

                    default:
                        Console.WriteLine($"[SubscriptionEvents] Unhandled subscription event subject: {subscriptionEvent.Subject}");
                        await args.DeadLetterMessageAsync(args.Message); // Dead-letter if action is not recognized
                        break;
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubscriptionEvents] Error processing message: {ex.Message}");
                await args.DeadLetterMessageAsync(args.Message); // Dead-letter on error
            }
        }

        private async Task HandleUserCreated(NotificationMessage notification)
        {
            Console.WriteLine($"Processing user creation for: {notification?.RecipientEmail ?? "undefined"}");
            string subject = "Your Account Has Been Created";
            string body = notification.Message; // Directly using the message

            await _emailService.SendEmailAsync(notification?.RecipientEmail ?? "undefined@domain.com", subject, body);
        }

        private async Task HandleRolesAssigned(NotificationMessage notification)
        {
            Console.WriteLine($"Processing roles assignment for: {notification?.RecipientEmail ?? "undefined"}");
            string subject = "Your Roles Have Been Assigned";
            string body = notification.Message; // Directly using the message

            await _emailService.SendEmailAsync(notification?.RecipientEmail ?? "undefined@domain.com", subject, body);
        }

        private async Task HandleSubscriptionActivated(NotificationMessage notification)
        {
            Console.WriteLine($"Processing subscription activation for: {notification?.RecipientEmail ?? "undefined"}");
            string subject = "Subscription Activated";
            string body = notification.Message; // Directly using the message

            await _emailService.SendEmailAsync(notification?.RecipientEmail ?? "undefined@domain.com", subject, body);
        }

        private async Task HandleSubscriptionUpdated(NotificationMessage notification)
        {
            Console.WriteLine($"Processing subscription update for: {notification?.RecipientEmail ?? "undefined"}");
            string subject = "Your Subscription Has Been Updated";
            string body = notification.Message; // Directly using the message

            await _emailService.SendEmailAsync(notification?.RecipientEmail ?? "undefined@domain.com", subject, body);
        }

        private async Task HandleSubscriptionCanceled(NotificationMessage notification)
        {
            Console.WriteLine($"Processing subscription cancellation for: {notification?.RecipientEmail ?? "undefined"}");
            string subject = "Your Subscription Has Been Canceled";
            string body = notification.Message; // Directly using the message

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
