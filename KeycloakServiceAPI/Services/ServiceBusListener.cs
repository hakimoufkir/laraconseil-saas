using Azure.Messaging.ServiceBus;
using System.Text.Json;
using KeycloakServiceAPI.Models;

namespace KeycloakServiceAPI.Services
{
    public class ServiceBusListener
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly KeycloakService _keycloakService;

        public ServiceBusListener(ServiceBusClient serviceBusClient, KeycloakService keycloakService)
        {
            _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
            _keycloakService = keycloakService ?? throw new ArgumentNullException(nameof(keycloakService));
        }

        public async Task StartAsync()
        {
            var processor = _serviceBusClient.CreateProcessor("keycloak-actions", "keycloak-service-sub", new ServiceBusProcessorOptions());
            processor.ProcessMessageAsync += ProcessMessageAsync;
            processor.ProcessErrorAsync += ProcessErrorAsync;

            await processor.StartProcessingAsync();
            Console.WriteLine("Keycloak Service Bus Listener started for 'keycloak-actions' topic with subscription 'keycloak-service-sub'.");
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            try
            {
                var messageBody = args.Message.Body.ToString();
                Console.WriteLine($"[Keycloak-Actions] Received message: {messageBody}");

                KeycloakActionMessage? message = null;

                try
                {
                    message = JsonSerializer.Deserialize<KeycloakActionMessage>(messageBody);
                    Console.WriteLine($"[Keycloak-Actions] Deserialized message: Action={message?.Action}, TenantName={message?.Data?.TenantName}, TenantEmail={message?.Data?.TenantEmail}, PlanType={message?.Data?.PlanType}");
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[Keycloak-Actions] Deserialization error: {ex.Message}");
                    await args.DeadLetterMessageAsync(args.Message); // Dead-letter the message if deserialization fails
                    return;
                }

                if (message?.Action == "CreateUserWithoutRoles")
                {
                    if (message.Data?.TenantName == null || message.Data?.TenantEmail == null)
                    {
                        Console.WriteLine("[Keycloak-Actions] Invalid message data: TenantName or TenantEmail is null.");
                        await args.DeadLetterMessageAsync(args.Message); // Dead-letter if data is invalid
                        return;
                    }

                    Console.WriteLine($"[Keycloak-Actions] Processing 'CreateUserWithoutRoles' for Client: {message.Data.TenantName}, Tenant: {message.Data.TenantEmail}, PlanType: {message.Data.PlanType}");
                    await _keycloakService.CreateUserWithoutRolesAsync(message.Data.TenantName, message.Data.TenantEmail);
                }
                else if (message?.Action == "AssignRolesToUser")
                {
                    if (message.Data?.TenantEmail == null || message.Data?.PlanType == null)
                    {
                        Console.WriteLine("[Keycloak-Actions] Invalid message data: TenantEmail or PlanType is null.");
                        await args.DeadLetterMessageAsync(args.Message); // Dead-letter if data is invalid
                        return;
                    }

                    Console.WriteLine($"[Keycloak-Actions] Processing 'AssignRolesToUser' for Tenant: {message.Data.TenantEmail}, PlanType: {message.Data.PlanType}");
                    await _keycloakService.AssignRolesToUserAsync(message.Data.TenantEmail, message.Data.PlanType);
                }
                else
                {
                    Console.WriteLine("[Keycloak-Actions] Action not recognized: " + message?.Action);
                    await args.DeadLetterMessageAsync(args.Message); // Dead-letter if action is not recognized
                }

                // Complete the message after successful processing
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Keycloak-Actions] Processing Error: {ex.Message}");
                await args.DeadLetterMessageAsync(args.Message); // Dead-letter on error
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"[Service Bus Error] {args.Exception.Message}");
            return Task.CompletedTask;
        }
    }
}
