using MediatR;
using Stripe;

namespace MultiTenantStripeAPI.Application.Features.Payment.Commands
{
    public class ProcessStripeEventCommand : IRequest<Unit>
    {
        public Event StripeEvent { get; set; }

        public ProcessStripeEventCommand(Event stripeEvent)
        {
            StripeEvent = stripeEvent ?? throw new ArgumentNullException(nameof(stripeEvent));
        }
    }
}
