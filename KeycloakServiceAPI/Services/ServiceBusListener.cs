using Azure.Messaging.ServiceBus;
using KeycloakServiceAPI.Models;

namespace KeycloakServiceAPI.Services;

public class ServiceBusListener
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly KeycloakService _keycloakService;

    public ServiceBusListener(ServiceBusClient serviceBusClient, KeycloakService keycloakService)
    {
        _serviceBusClient = serviceBusClient;
        _keycloakService = keycloakService;
    }

    public async Task StartAsync()
    {
        var processor = _serviceBusClient.CreateProcessor("keycloak-actions", new ServiceBusProcessorOptions());
        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ProcessErrorAsync;

        await processor.StartProcessingAsync();
        Console.WriteLine("Keycloak Service Bus Listener started.");
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var messageBody = args.Message.Body.ToString();
            var message = System.Text.Json.JsonSerializer.Deserialize<KeycloakActionMessage>(messageBody);

            if (message?.Action == "CreateRealmAndUser")
            {
                await _keycloakService.CreateRealmAndUserAsync(message.Data.RealmName, message.Data.TenantEmail);
            }

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Service Bus Error: {args.Exception.Message}");
        return Task.CompletedTask;
    }
}
