using MultiTenantStripeAPI.Domain.Entities;

namespace MultiTenantStripeAPI.Application.IServices
{
    public interface IKeycloakService
    {
        Task CreateRealmAndUserAsync(KeycloakActionData keycloakActionData);
    }
}