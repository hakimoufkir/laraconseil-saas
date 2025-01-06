namespace MultiTenantStripeAPI.Application.IServices
{
    public interface IKeycloakService
    {
        Task CreateRealmAndUserAsync(string realmName, string tenantEmail);
    }
}