using MultiTenantStripeAPI.Application.IServices;
using MultiTenantStripeAPI.Domain.Entities;

namespace MultiTenantStripeAPI.Application.Services
{
    public class KeycloakService : IKeycloakService
    {
        private readonly IServiceBusPublisher _serviceBusPublisher;

        public KeycloakService(IServiceBusPublisher serviceBusPublisher)
        {
            _serviceBusPublisher = serviceBusPublisher ?? throw new ArgumentNullException(nameof(serviceBusPublisher));
        }

        public async Task CreateRealmAndUserAsync(KeycloakActionData keycloakActionData)
        {
            // Validate inputs
            if (keycloakActionData is null)
            {
                Console.WriteLine("Error: keycloakActionData is required.");
                throw new ArgumentException("Error: keycloakActionData is required.", nameof(keycloakActionData));
            }
            // Construct the message
            KeycloakActionMessage message = new()
            {
                Action = "AssignRolesToUser",
                Data = keycloakActionData
            };

            try
            {
                // Serialize and publish the message
                await _serviceBusPublisher.PublishMessageAsync("keycloak-actions", message);
                Console.WriteLine($"[KeycloakService] Action published: keycloakActionData '{keycloakActionData}'.");
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"[KeycloakService] Error publishing message for keycloakActionData '{keycloakActionData}',{ex.Message}");
                throw;
            }
        }
    }
}
