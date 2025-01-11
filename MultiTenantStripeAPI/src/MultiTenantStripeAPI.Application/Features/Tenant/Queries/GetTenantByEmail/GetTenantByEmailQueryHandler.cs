using MediatR;
using MultiTenantStripeAPI.Application.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.Features.Tenant.Queries.GetTenantByEmail
{
    public class GetTenantByEmailQueryHandler : IRequestHandler<GetTenantByEmailQuery, MultiTenantStripeAPI.Domain.Entities.Tenant>
    {
        private readonly ITenantService _tenantService;

        public GetTenantByEmailQueryHandler(ITenantService tenantService)
        {
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        }

        public async Task<MultiTenantStripeAPI.Domain.Entities.Tenant> Handle(GetTenantByEmailQuery request, CancellationToken cancellationToken)
        {
            return _tenantService.GetTenantByEmail(request.Email);
        }
    }
}
