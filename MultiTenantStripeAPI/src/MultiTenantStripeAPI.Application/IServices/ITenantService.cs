using MultiTenantStripeAPI.Domain.Entities;

namespace MultiTenantStripeAPI.Application.IServices
{
    public interface ITenantService
    {
        Tenant CreateTenant(string tenantName, string email);
        Tenant GetTenantByEmail(string email);
        Tenant GetTenantById(string tenantId);
        void UpdateTenantStatus(Tenant tenant, string status);
        void DeleteTenant(string tenantId);
    }
}
