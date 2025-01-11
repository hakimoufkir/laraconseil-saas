using MediatR;
using MultiTenantStripeAPI.Application.IServices;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.Features.Payment.Queries.ValidateTenant
{
    public class ValidateTenantQueryHandler : IRequestHandler<ValidateTenantQuery, bool>
    {
        private readonly ITenantService _tenantService;

        public ValidateTenantQueryHandler(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        public Task<bool> Handle(ValidateTenantQuery request, CancellationToken cancellationToken)
        {
            var tenant = _tenantService.GetTenantById(request.TenantId);
            return Task.FromResult(tenant != null);
        }
    }
}
