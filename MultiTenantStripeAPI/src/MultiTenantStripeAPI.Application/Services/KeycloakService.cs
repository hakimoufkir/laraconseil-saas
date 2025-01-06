using MultiTenantStripeAPI.Application.IServices;
using MultiTenantStripeAPI.Domain.Entities;
using System.Text.Json;

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
            if (string.IsNullOrWhiteSpace(realmName)) throw new ArgumentException("Realm name must be provided.", nameof(realmName));
            if (string.IsNullOrWhiteSpace(tenantEmail)) throw new ArgumentException("Tenant email must be provided.", nameof(tenantEmail));

            var message = new KeycloakActionMessage
            {
                Action = "CreateRealmAndUser",
                Data = new KeycloakActionData
                {
                    RealmName = realmName,
                    TenantEmail = tenantEmail
                }
            };

            var messageJson = JsonSerializer.Serialize(message);
            await _serviceBusPublisher.PublishMessageAsync("keycloak-actions", messageJson);
            Console.WriteLine($"Keycloak action published for realm '{realmName}' and tenant '{tenantEmail}'.");
        }
    }
}
