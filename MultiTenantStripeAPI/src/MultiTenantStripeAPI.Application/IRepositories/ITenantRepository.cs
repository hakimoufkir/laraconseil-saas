using MultiTenantStripeAPI.Application.IGenericRepo;
using MultiTenantStripeAPI.Domain.Entities;

namespace MultiTenantStripeAPI.Application.IRepositories
{
    public interface ITenantRepository : IGenericRepository<Tenant>
    {
        Tenant GetByEmail(string email);
        Tenant GetByTenantId(string tenantId);
    }
}
