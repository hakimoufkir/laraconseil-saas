using MediatR;
using MultiTenantStripeAPI.Application.Features.Payment.Commands.CreateCheckoutSession;
using MultiTenantStripeAPI.Application.IServices;
using MultiTenantStripeAPI.Domain.Entities;


namespace MultiTenantStripeAPI.Application.Features.Payment.Commands
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
            Tenant tenant;

            try
            {
                // Check if a tenant with the provided email already exists
                tenant = _tenantService.GetTenantByEmail(request.Email);
                Console.WriteLine($"Tenant with email {request.Email} already exists.");
            }
            catch (InvalidOperationException)
            {
                // If the tenant does not exist, create a new one
                Console.WriteLine($"Tenant with email {request.Email} does not exist. Creating a new tenant.");
                tenant = _tenantService.CreateTenant(request.TenantName, request.Email);
            }

            // Create a Stripe checkout session for the tenant
            var session = _stripeService.CreateCheckoutSession(tenant.TenantName, tenant.Email);

            // Return the session ID for further processing or redirection
            return session.Id;
        }
    }
}
