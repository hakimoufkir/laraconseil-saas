namespace MultiTenantStripeAPI.Application.IServices
{
    public interface IKeycloakService
    {
        Task<string> GetAdminTokenAsync();
        Task CreateRealmAndUserAsync(string realmName, string tenantEmail);
    }
}