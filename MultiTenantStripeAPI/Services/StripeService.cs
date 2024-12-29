using Microsoft.Extensions.Options;
using MultiTenantStripeAPI.Models;
using MultiTenantStripeAPI.Services.interfaces;
using MultiTenantStripeAPI.Services.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace MultiTenantStripeAPI.Services
{
    public class StripeService : IStripeService
    {
        private readonly StripeOptions _stripeOptions;
        private readonly ITenantService _tenantService;
        private readonly IKeycloakService _keycloakService;

        public StripeService(IOptions<StripeOptions> stripeOptions, ITenantService tenantService, IKeycloakService keycloakService)
        {
            _stripeOptions = stripeOptions.Value ?? throw new ArgumentNullException(nameof(stripeOptions));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _keycloakService = keycloakService ?? throw new ArgumentNullException(nameof(keycloakService));
            StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
        }

        public Session CreateCheckoutSession(string tenantName, string email)
        {
            // Create or retrieve a customer
            var customerService = new CustomerService();
            var existingCustomer = customerService.List(new CustomerListOptions { Email = email }).Data.FirstOrDefault();
            var customerId = existingCustomer?.Id ?? customerService.Create(new CustomerCreateOptions { Email = email }).Id;

            var options = new SessionCreateOptions
            {
                Customer = customerId, // Associate session with customer
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                Price = "price_1Qb9QMIQYGMLN4GqsyWZd0VH",
                Quantity = 1,
            },
        },
                Mode = "subscription",
                SuccessUrl = "http://localhost:4200/success",
                CancelUrl = "http://localhost:4200/cancel",
                // SuccessUrl = "https://subscription-app.gentlegrass-3889baac.westeurope.azurecontainerapps.io/success",
                // CancelUrl = "https://subscription-app.gentlegrass-3889baac.westeurope.azurecontainerapps.io/cancel",
            };

            var service = new SessionService();
            return service.Create(options);
        }

        public async Task HandleCheckoutSessionCompleted(Event stripeEvent)
        {
            if (stripeEvent?.Data?.Object is not Session session)
            {
                Console.WriteLine("Stripe session is null or invalid.");
                return;
            }

            // Check for Customer ID or Customer Email
            string customerId = session.CustomerId;
            string customerEmail = session.CustomerDetails?.Email;

            if (string.IsNullOrWhiteSpace(customerId) && string.IsNullOrWhiteSpace(customerEmail))
            {
                Console.WriteLine("Both Customer ID and Customer Email are missing in the session.");
                return;
            }

            Console.WriteLine($"Checkout Session Completed. Customer ID: {customerId}, Email: {customerEmail}");

            // If Customer ID is missing, use Customer Email for further processing
            if (string.IsNullOrWhiteSpace(customerId) && !string.IsNullOrWhiteSpace(customerEmail))
            {
                Console.WriteLine($"Customer ID is missing. Proceeding with Customer Email: {customerEmail}");
            }

            // Ensure we have a valid email for tenant operations
            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                Console.WriteLine("Customer Email is missing, cannot proceed.");
                return;
            }

            // Retrieve or create tenant
            Tenant tenant;
            try
            {
                tenant = _tenantService.GetTenantByEmail(customerEmail);
                Console.WriteLine($"Tenant found for email: {customerEmail}");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine($"No tenant found with email '{customerEmail}'. Creating a new tenant.");
                string tenantName = session.CustomerDetails?.Name ?? "Unknown Tenant";
                tenant = _tenantService.CreateTenant(tenantName, customerEmail);
            }

            // Update subscription status
            _tenantService.UpdateTenantStatus(tenant, "Active");
            Console.WriteLine("Tenant subscription status updated to Active.");

            // Create realm and user in Keycloak
            try
            {
                await _keycloakService.CreateRealmAndUserAsync(tenant.TenantName, tenant.Email);
                Console.WriteLine("Keycloak realm and user created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Keycloak realm and user: {ex.Message}");
            }
        }


        public async Task HandleSubscriptionUpdated(Event stripeEvent)
        {
            if (stripeEvent?.Data?.Object is not Subscription subscription)
            {
                Console.WriteLine("Stripe subscription is null or invalid.");
                return;
            }

            string customerId = subscription.CustomerId;
            var customerService = new CustomerService();
            var customer = await customerService.GetAsync(customerId);
            string customerEmail = customer?.Email;

            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                Console.WriteLine("Customer email not found for subscription update.");
                return;
            }

            Console.WriteLine($"Subscription updated for: {customerEmail}, Status: {subscription.Status}");

            var tenant = _tenantService.GetTenantByEmail(customerEmail);
            if (tenant == null)
            {
                Console.WriteLine($"Tenant not found for email: {customerEmail}");
                return;
            }

            // Update tenant subscription status
            _tenantService.UpdateTenantStatus(tenant, subscription.Status);
            Console.WriteLine($"Tenant subscription status updated to: {subscription.Status}");
        }

        public async Task HandleSubscriptionDeleted(Event stripeEvent)
        {
            if (stripeEvent?.Data?.Object is not Subscription subscription)
            {
                Console.WriteLine("Stripe subscription is null or invalid.");
                return;
            }

            string customerId = subscription.CustomerId;
            var customerService = new CustomerService();
            var customer = await customerService.GetAsync(customerId);
            string customerEmail = customer?.Email;

            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                Console.WriteLine("Customer email not found for subscription deletion.");
                return;
            }

            Console.WriteLine($"Subscription deleted for: {customerEmail}");

            var tenant = _tenantService.GetTenantByEmail(customerEmail);
            if (tenant == null)
            {
                Console.WriteLine($"Tenant not found for email: {customerEmail}");
                return;
            }

            // Update tenant subscription status
            _tenantService.UpdateTenantStatus(tenant, "Canceled");
            Console.WriteLine("Tenant subscription status updated to Canceled.");
        }
    }
}
