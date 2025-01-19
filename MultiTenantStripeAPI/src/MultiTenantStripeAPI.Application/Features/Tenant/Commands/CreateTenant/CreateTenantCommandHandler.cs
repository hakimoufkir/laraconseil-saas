using MediatR;
using MultiTenantStripeAPI.Application.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.Features.Tenant.Commands.CreateTenant
{
    public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, string>
    {
        private readonly ITenantService _tenantService;

        public CreateTenantCommandHandler(ITenantService tenantService)
        {
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        }

        public async Task<string> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
        {
            var tenant = await _tenantService.CreateTenantAsync(request.TenantId, request.TenantName, request.Email, request.PlanType);
            return tenant.TenantId; // Return the Tenant ID as a response
        }
    }
}
