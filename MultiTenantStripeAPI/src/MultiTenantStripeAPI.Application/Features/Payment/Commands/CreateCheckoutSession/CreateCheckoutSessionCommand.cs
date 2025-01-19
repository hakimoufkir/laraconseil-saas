using MediatR;

namespace MultiTenantStripeAPI.Application.Features.Payment.Commands.CreateCheckoutSession
{
   public class CreateCheckoutSessionCommand : IRequest<string>
    {
        public string TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;
    }
}
