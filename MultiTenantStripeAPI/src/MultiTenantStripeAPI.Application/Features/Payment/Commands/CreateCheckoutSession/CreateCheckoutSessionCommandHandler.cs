using MediatR;
using MultiTenantStripeAPI.Application.IServices;
using MultiTenantStripeAPI.Domain.Entities;
using Stripe;

namespace MultiTenantStripeAPI.Application.Features.Payment.Commands.CreateCheckoutSession
{
    public class CreateCheckoutSessionCommandHandler : IRequestHandler<CreateCheckoutSessionCommand, string>
    {
        private readonly ITenantService _tenantService;
        private readonly IStripeService _stripeService;

        public CreateCheckoutSessionCommandHandler(
            ITenantService tenantService,
            IStripeService stripeService)
        {
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _stripeService = stripeService ?? throw new ArgumentNullException(nameof(stripeService));
        }

        public async Task<string> Handle(CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.PlanType))
                throw new ArgumentException("Plan type must be provided.", nameof(request.PlanType));

            MultiTenantStripeAPI.Domain.Entities.Tenant tenant;

            try
            {
                // Create Stripe checkout session for the tenant with metadata
                var session = _stripeService.CreateCheckoutSession(request.TenantId, request.TenantName, request.Email, request.PlanType);

                // Session ID is returned here, which can be used to redirect the customer to the Stripe Checkout page
                Console.WriteLine($"Created Stripe Checkout session for tenant: {request.TenantName}, email: {request.Email}, plan: {request.PlanType}");

                // Return the session ID for redirection to Stripe Checkout
                return session.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw; // Re-throw the exception for the outer handler to deal with
            }
        }
    }
}
