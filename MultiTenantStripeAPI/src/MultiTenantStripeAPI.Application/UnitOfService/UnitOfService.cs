using MultiTenantStripeAPI.Application.IServices;

namespace MultiTenantStripeAPI.Application.UnitOfService
{
    public class UnitOfService : IUnitOfService.IUnitOfService
    {
        public UnitOfService(ITenantService tenantService)
        {
            TenantService = tenantService;
        }

        public ITenantService TenantService { get; }
    }
}
