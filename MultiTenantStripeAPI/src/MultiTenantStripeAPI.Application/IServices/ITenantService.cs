using MultiTenantStripeAPI.Domain.Entities;

namespace MultiTenantStripeAPI.Application.IServices
{
    public interface ITenantService
    {
        Task<Tenant> CreateTenantAsync(string tenantId, string tenantName, string email, string planType);
        Task<Tenant> GetTenantByEmailAsync(string email);
        Task<Tenant> GetTenantByIdAsync(string tenantId);
        Task UpdateTenantStatusAsync(Tenant tenant, string status);
        Task DeleteTenantAsync(string tenantId);
        Task<Tenant> GetTenantByEmail(string email);
        Task<Tenant> GetTenantById(string tenantId);
    }
}

