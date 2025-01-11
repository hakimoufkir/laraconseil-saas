using MediatR;

namespace MultiTenantStripeAPI.Application.Features.Payment.Queries.ValidateTenant
{
    public class ValidateTenantQuery : IRequest<bool>
    {
        public string TenantId { get; set; }
    }
}
