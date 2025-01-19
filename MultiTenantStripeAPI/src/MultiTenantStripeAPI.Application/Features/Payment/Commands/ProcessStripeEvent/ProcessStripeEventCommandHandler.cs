using MediatR;
using MultiTenantStripeAPI.Application.IServices;
using Stripe;
using Stripe.Checkout;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.Features.Payment.Commands.ProcessStripeEvent
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

            Console.WriteLine($"Handling Stripe event of type: {stripeEvent.Type}");

            // Handle supported Stripe events
            try
            {
                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        // Log the stripe event and inspect the metadata
                        var session = stripeEvent.Data.Object as Session;

                        if (session != null)
                        {
                            Console.WriteLine("Stripe Event: checkout.session.completed");
                            Console.WriteLine($"Session Metadata: {session.Metadata}");

                            // Extract the necessary metadata
                            var tenantId = session.Metadata.ContainsKey("TenantId") ? session.Metadata["TenantId"] : string.Empty;
                            var planType = session.Metadata.ContainsKey("PlanType") ? session.Metadata["PlanType"] : string.Empty;
                            var email = session.Metadata.ContainsKey("Email") ? session.Metadata["Email"] : string.Empty;

                            Console.WriteLine($"Extracted Metadata - TenantId: {tenantId}, PlanType: {planType}, Email: {email}");

                            // Pass the stripe event and the metadata to the service for further processing
                            await _stripeService.HandleCheckoutSessionCompleted(stripeEvent); // Keep the original method signature
                        }
                        else
                        {
                            Console.WriteLine("[ERROR] Session data is missing or invalid.");
                        }
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing Stripe event of type {stripeEvent.Type}: {ex.Message}");
                throw;  // Rethrow the exception to ensure it can be handled upstream
            }

            return Unit.Value;
        }
    }
}
