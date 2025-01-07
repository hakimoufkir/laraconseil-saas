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

        public async Task CreateRealmAndUserAsync(string realmName, string tenantEmail)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(realmName))
            {
                Console.WriteLine("Error: Realm name is required.");
                throw new ArgumentException("Realm name must be provided.", nameof(realmName));
            }

            if (string.IsNullOrWhiteSpace(tenantEmail))
            {
                Console.WriteLine("Error: Tenant email is required.");
                throw new ArgumentException("Tenant email must be provided.", nameof(tenantEmail));
            }

            // Construct the message
            KeycloakActionMessage message = new()
            {
                Action = "CreateRealmAndUser",
                Data = new KeycloakActionData
                {
                    RealmName = realmName,
                    TenantEmail = tenantEmail
                }
            };

            try
            {
                // Serialize and publish the message
                await _serviceBusPublisher.PublishMessageAsync("keycloak-actions", message);
                Console.WriteLine($"[KeycloakService] Action published: Realm '{realmName}', Tenant '{tenantEmail}'.");
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"[KeycloakService] Error publishing message for realm '{realmName}', tenant '{tenantEmail}': {ex.Message}");
                throw;
            }
        }
    }
}
