using MediatR;
using MultiTenantStripeAPI.Application.IServices;
using Stripe;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.Features.Payment.Commands
{
    public class ProcessStripeEventCommandHandler : IRequestHandler<ProcessStripeEventCommand, Unit>
    {
        private readonly IStripeService _stripeService;

        public ProcessStripeEventCommandHandler(IStripeService stripeService)
        {
            _stripeService = stripeService ?? throw new ArgumentNullException(nameof(stripeService));
        }

        public async Task<Unit> Handle(ProcessStripeEventCommand request, CancellationToken cancellationToken)
        {
            if (request.StripeEvent == null)
            {
                throw new ArgumentNullException(nameof(request.StripeEvent), "Stripe event cannot be null.");
            }

            var stripeEvent = request.StripeEvent;

            // Handle supported Stripe events
            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    await _stripeService.HandleCheckoutSessionCompleted(stripeEvent);
                    break;
                case "customer.subscription.updated":
                    await _stripeService.HandleSubscriptionUpdated(stripeEvent);
                    break;
                case "customer.subscription.deleted":
                    await _stripeService.HandleSubscriptionDeleted(stripeEvent);
                    break;
                default:
                    Console.WriteLine($"Unhandled Stripe event type: {stripeEvent.Type}");
                    break;
            }

            return Unit.Value;
        }
    }
}
