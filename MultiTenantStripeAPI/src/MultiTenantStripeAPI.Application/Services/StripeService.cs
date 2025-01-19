using Microsoft.Extensions.Options;
using MultiTenantStripeAPI.Application.IServices;
using MultiTenantStripeAPI.Domain.Entities;
using Stripe;
using Stripe.Checkout;
using System.Threading.Tasks;
using System;

namespace MultiTenantStripeAPI.Application.Services
{
    public class StripeService : IStripeService
    {
        private readonly StripeOptions _stripeOptions;
        private readonly ITenantService _tenantService;
        private readonly IKeycloakService _keycloakService;
        private readonly IServiceBusPublisher _serviceBusPublisher;

        public StripeService(
            IOptions<StripeOptions> stripeOptions,
            ITenantService tenantService,
            IKeycloakService keycloakService,
            IServiceBusPublisher serviceBusPublisher)
        {
            _stripeOptions = stripeOptions?.Value ?? throw new ArgumentNullException(nameof(stripeOptions));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _keycloakService = keycloakService ?? throw new ArgumentNullException(nameof(keycloakService));
            _serviceBusPublisher = serviceBusPublisher ?? throw new ArgumentNullException(nameof(serviceBusPublisher));
            StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
        }

        // Create a Stripe checkout session
        public Session CreateCheckoutSession(string tenantId, string tenantName, string email, string planType)
        {
            if (string.IsNullOrWhiteSpace(tenantName)) throw new ArgumentException("Tenant name must be provided.", nameof(tenantName));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email must be provided.", nameof(email));
            if (string.IsNullOrWhiteSpace(planType)) throw new ArgumentException("Plan type must be provided.", nameof(planType));

            Console.WriteLine($"Creating checkout session for tenant: {tenantName}, email: {email}, plan: {planType}");

            var customerId = GetOrCreateCustomerId(email);

            var options = new SessionCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = GetPriceIdForPlan(planType),
                        Quantity = 1
                    }
                },
                Mode = "subscription",
                SuccessUrl = "https://webapp.gentlegrass-3889baac.westeurope.azurecontainerapps.io/success",
                CancelUrl = "https://webapp.gentlegrass-3889baac.westeurope.azurecontainerapps.io/cancel",
                Metadata = new Dictionary<string, string>
                {
                    { "Email", email },
                    { "PlanType", planType },
                    { "TenantId", tenantId },
                    { "TenantName", tenantName }
                }
            };

            // Log metadata to verify it's being passed
            Console.WriteLine($"Stripe metadata: TenantId={tenantId}, PlanType={planType}, Email={email}");

            var session = new SessionService().Create(options);
            Console.WriteLine($"Stripe session created successfully: {session.Id}");
            return session;
        }



        // Handle the checkout session completed event
        public async Task HandleCheckoutSessionCompleted(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;

            // Retrieve metadata from the session
            var email = session.CustomerDetails?.Email;
            var tenantId = session.Metadata["TenantId"];  // Retrieve Tenant ID from metadata
            var tenantName = session.Metadata["TenantName"];  // Retrieve Tenant Name from metadata
            var planType = session.Metadata["PlanType"];  // Retrieve PlanType from metadata

            Console.WriteLine($"Metadata - TenantId: {tenantId}, TenantName: {tenantName}, PlanType: {planType}");


            // Validate the presence of required metadata
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(tenantName) || string.IsNullOrWhiteSpace(planType))
            {
                Console.WriteLine("[ERROR] Missing tenant details in metadata. Make sure 'TenantId', 'TenantName', and 'PlanType' are included.");
                return;
            }

            Console.WriteLine($"[INFO] Tenant ID: {tenantId}, Tenant Name: {tenantName}, Plan Type: {planType}");

            // Create or retrieve the tenant using the new data
            var tenant = await _tenantService.CreateTenantAsync(tenantId, tenantName, email, planType);

            // Update tenant status to Active
            await _tenantService.UpdateTenantStatusAsync(tenant, "Active");

            try
            {
                KeycloakActionData keycloakActionData = new KeycloakActionData
                {
                    TenantName = tenantName, // hadi ma3ndha hta chi m3na
                    TenantEmail = email,
                    PlanType = planType
                };
                // Send message to KeycloakServiceAPI to AssignRolesToUser
                await _keycloakService.CreateRealmAndUserAsync(keycloakActionData);
                Console.WriteLine($"[INFO] A message sent to KeycloakServiceAPI to AssignRolesToUser for user name name =  '{tenant.TenantName}'.");

                // Send notification for realm creation
                //await PublishNotification("Realm Created", tenant.Email, tenant.TenantName, "Your realm has been successfully created.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send a message to KeycloakServiceAPI for tenant '{tenant.TenantName}': {ex.Message}");
            }
        }


        // Handle subscription updated event
        public async Task HandleSubscriptionUpdated(Event stripeEvent)
        {
            if (stripeEvent?.Data?.Object is not Subscription subscription)
            {
                Console.WriteLine("[ERROR] Invalid Stripe event or missing subscription data.");
                return;
            }

            var email = await GetCustomerEmailAsync(subscription.CustomerId);
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("[ERROR] Customer email is missing from the subscription.");
                return;
            }

            MultiTenantStripeAPI.Domain.Entities.Tenant tenant = await _tenantService.GetTenantByEmail(email);
            if (tenant == null)
            {
                Console.WriteLine($"[ERROR] No tenant found for email '{email}'.");
                return;
            }

            // Update tenant subscription status
            await UpdateTenantSubscriptionStatus(tenant, subscription.Status);
        }

        // Handle subscription deleted event
        public async Task HandleSubscriptionDeleted(Event stripeEvent)
        {
            if (stripeEvent?.Data?.Object is not Subscription subscription)
            {
                Console.WriteLine("[ERROR] Invalid Stripe event or missing subscription data.");
                return;
            }

            var email = await GetCustomerEmailAsync(subscription.CustomerId);
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("[ERROR] Customer email is missing from the subscription.");
                return;
            }

            MultiTenantStripeAPI.Domain.Entities.Tenant tenant = await _tenantService.GetTenantByEmail(email);
            if (tenant == null)
            {
                Console.WriteLine($"[ERROR] No tenant found for email '{email}'.");
                return;
            }

            // Update tenant subscription status to Canceled
            await UpdateTenantSubscriptionStatus(tenant, "Canceled");
        }

        // Get price ID based on the plan type
        private string GetPriceIdForPlan(string planType)
        {
            return planType switch
            {
                "Grower" => "price_1QhGC3IQYGMLN4Gq8Zh4PGYK", // Make sure this is correct
                "Station" => "price_1QhGCLIQYGMLN4GqW1cvSqQo", // Make sure this is correct
                _ => throw new ArgumentException("Invalid plan type.", nameof(planType))
            };
        }

        // Get or create customer in Stripe
        private string GetOrCreateCustomerId(string email)
        {
            var customerService = new CustomerService();
            var existingCustomer = customerService.List(new CustomerListOptions { Email = email }).Data.FirstOrDefault();
            return existingCustomer?.Id ?? customerService.Create(new CustomerCreateOptions { Email = email }).Id;
        }

        // Get the customer's email using their Stripe customer ID
        private async Task<string?> GetCustomerEmailAsync(string customerId)
        {
            var customerService = new CustomerService();
            var customer = await customerService.GetAsync(customerId);
            return customer?.Email;
        }


        // Update tenant subscription status
        private async Task UpdateTenantSubscriptionStatus(Tenant tenant, string status)
        {
            await _tenantService.UpdateTenantStatusAsync(tenant, status);
            Console.WriteLine($"[INFO] Tenant '{tenant.TenantName}' subscription status updated to: {status}");

            await PublishSubscriptionEvent($"Subscription {status}", tenant.TenantName, tenant.Email, status);
        }

        // Publish subscription event to the service bus
        private async Task PublishSubscriptionEvent(string subject, string tenantName, string email, string subscriptionStatus)
        {
            var notification = new NotificationMessage
            {
                Subject = subject,
                RecipientEmail = email,
                Message = $"Your subscription status is now {subscriptionStatus}." // Directly using the message field
            };

            await _serviceBusPublisher.PublishMessageAsync("subscription-events", notification);
            Console.WriteLine($"[INFO] Subscription event '{subject}' published for tenant '{tenantName}'.");
        }

    }
}
