using Azure.Messaging.ServiceBus;
using System.Text.Json;
using KeycloakServiceAPI.Models;

namespace KeycloakServiceAPI.Services;

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
        // Configure processor for keycloak-actions topic with keycloak-service-sub subscription
        var processor = _serviceBusClient.CreateProcessor("keycloak-actions", "keycloak-service-sub", new ServiceBusProcessorOptions());
        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ProcessErrorAsync;

        // Start listening
        await processor.StartProcessingAsync();
        Console.WriteLine("Keycloak Service Bus Listener started for 'keycloak-actions' topic with subscription 'keycloak-service-sub'.");
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var messageBody = args.Message.Body.ToString();
            Console.WriteLine($"[Keycloak-Actions] Received message: {messageBody}");

            KeycloakActionMessage message = null;

            try
            {
                message = System.Text.Json.JsonSerializer.Deserialize<KeycloakActionMessage>(messageBody);
                Console.WriteLine($"[Keycloak-Actions] Deserialized message: Action={message?.Action}, RealmName={message?.Data?.RealmName}, TenantEmail={message?.Data?.TenantEmail}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[Keycloak-Actions] Deserialization error: {ex.Message}");
            }
            if (message?.Action == "CreateRealmAndUser")
            {
                Console.WriteLine($"[Keycloak-Actions] Processing 'CreateRealmAndUser' for Realm: {message.Data.RealmName}, Tenant: {message.Data.TenantEmail}");
                await _keycloakService.CreateRealmAndUserAsync(message.Data.RealmName, message.Data.TenantEmail);
            }

            // Complete the message after successful processing
            await args.CompleteMessageAsync(args.Message);
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"[Keycloak-Actions] JSON Deserialization Error: {jsonEx.Message}");
            await args.DeadLetterMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Keycloak-Actions] Processing Error: {ex.Message}");
            await args.AbandonMessageAsync(args.Message); // Retry the message
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"[Service Bus Error] {args.Exception.Message}");
        return Task.CompletedTask;
    }
}
