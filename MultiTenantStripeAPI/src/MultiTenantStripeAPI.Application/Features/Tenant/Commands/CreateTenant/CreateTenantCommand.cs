using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.Features.Tenant.Commands.CreateTenant
{
    public class CreateTenantCommand : IRequest<string>
    {
        public string TenantId { get; set; }  // Add TenantId here
        public string TenantName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty; // Plan Type: Grower, Station
    }
}
