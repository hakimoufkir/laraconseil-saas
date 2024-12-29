namespace MultiTenantStripeAPI.Services.interfaces
{
     public interface IKeycloakService
    {
        Task<string> GetAdminTokenAsync();
        Task CreateRealmAndUserAsync(string realmName, string tenantEmail);
    }
}