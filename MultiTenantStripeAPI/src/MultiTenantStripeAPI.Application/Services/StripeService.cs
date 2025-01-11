using Microsoft.Extensions.Options;
using MultiTenantStripeAPI.Application.IServices;
using MultiTenantStripeAPI.Domain.Entities;
using Stripe;
using Stripe.Checkout;

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

        public Session CreateCheckoutSession(string tenantName, string email)
        {
            if (string.IsNullOrWhiteSpace(tenantName)) throw new ArgumentException("Tenant name must be provided.", nameof(tenantName));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email must be provided.", nameof(email));

            Console.WriteLine($"Creating checkout session for tenant: {tenantName}, email: {email}");

            var customerService = new CustomerService();
            var existingCustomer = customerService.List(new CustomerListOptions { Email = email }).Data.FirstOrDefault();
            var customerId = existingCustomer?.Id ?? customerService.Create(new CustomerCreateOptions { Email = email }).Id;

            if (string.IsNullOrEmpty(customerId))
                throw new InvalidOperationException("Failed to create or retrieve customer ID.");

            var options = new SessionCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = "price_1Qb9QMIQYGMLN4GqsyWZd0VH", // Replace with a valid price ID
                        Quantity = 1
                    }
                },
                Mode = "subscription",
                SuccessUrl = "https://subscription-app.gentlegrass-3889baac.westeurope.azurecontainerapps.io/success",
                CancelUrl = "https://subscription-app.gentlegrass-3889baac.westeurope.azurecontainerapps.io/cancel"
            };

            var session = new SessionService().Create(options);
            Console.WriteLine($"Session created successfully: {session.Id}");
            return session;
        }

        public async Task HandleCheckoutSessionCompleted(Event stripeEvent)
        {
            if (stripeEvent?.Data?.Object is not Session session)
            {
                Console.WriteLine("Invalid Stripe event or missing session data.");
                return;
            }

            var customerDetails = session.CustomerDetails;

            // Extract tenant details
            var tenantName = customerDetails?.Name ?? "Unknown Tenant";
            var tenantEmail = customerDetails?.Email;

            if (string.IsNullOrWhiteSpace(tenantEmail))
            {
                Console.WriteLine("Customer email is missing from the Stripe session.");
                return;
            }

            // Create or retrieve tenant
            var tenant = _tenantService.GetTenantByEmail(tenantEmail) ??
                         _tenantService.CreateTenant(tenantName, tenantEmail);

            _tenantService.UpdateTenantStatus(tenant, "Active");
            Console.WriteLine($"Tenant '{tenant.TenantName}' subscription status updated to Active.");

            // Publish subscription event
            await PublishSubscriptionEvent("Subscription Activated", tenant.TenantName, tenant.Email, "Active");

            try
            {
                // Trigger Keycloak realm and user creation
                await _keycloakService.CreateRealmAndUserAsync(tenant.TenantName, tenant.Email);
                Console.WriteLine($"Keycloak realm and user created successfully for tenant '{tenant.TenantName}'.");

                // Publish notification for realm creation
                await PublishNotification("Realm Created", tenant.Email, tenant.TenantName, "Your realm has been successfully created.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Keycloak realm for tenant '{tenant.TenantName}': {ex.Message}");
            }
        }

        public async Task HandleSubscriptionUpdated(Event stripeEvent)
        {
            if (stripeEvent?.Data?.Object is not Subscription subscription)
            {
                Console.WriteLine("Invalid Stripe event or missing subscription data.");
                return;
            }

            var customerService = new CustomerService();
            var customer = await customerService.GetAsync(subscription.CustomerId);
            var customerEmail = customer?.Email;

            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                Console.WriteLine("Customer email is missing from the subscription.");
                return;
            }

            var tenant = _tenantService.GetTenantByEmail(customerEmail);
            if (tenant == null)
            {
                Console.WriteLine($"No tenant found for email '{customerEmail}'.");
                return;
            }

            _tenantService.UpdateTenantStatus(tenant, subscription.Status);
            Console.WriteLine($"Tenant '{tenant.TenantName}' subscription status updated to: {subscription.Status}");

            await PublishSubscriptionEvent("Subscription Updated", tenant.TenantName, tenant.Email, subscription.Status);
        }

        public async Task HandleSubscriptionDeleted(Event stripeEvent)
        {
            if (stripeEvent?.Data?.Object is not Subscription subscription)
            {
                Console.WriteLine("Invalid Stripe event or missing subscription data.");
                return;
            }

            var customerService = new CustomerService();
            var customer = await customerService.GetAsync(subscription.CustomerId);
            var customerEmail = customer?.Email;

            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                Console.WriteLine("Customer email is missing from the subscription.");
                return;
            }

            var tenant = _tenantService.GetTenantByEmail(customerEmail);
            if (tenant == null)
            {
                Console.WriteLine($"No tenant found for email '{customerEmail}'.");
                return;
            }

            _tenantService.UpdateTenantStatus(tenant, "Canceled");
            Console.WriteLine($"Tenant '{tenant.TenantName}' subscription status updated to Canceled.");

            await PublishSubscriptionEvent("Subscription Canceled", tenant.TenantName, tenant.Email, "Canceled");
        }

        private async Task PublishNotification(string subject, string recipientEmail, string tenantName, string message)
        {
            try
            {
                var notification = new NotificationMessage
                {
                    Subject = subject,
                    RecipientEmail = recipientEmail,
                    Payload = new NotificationPayload
                    {
                        TenantName = tenantName ?? "Unknown Tenant",
                        Realm = tenantName ?? "Unknown Realm", // Assuming Realm is same as TenantName
                        Message = message ?? "No additional details available."
                    }
                };

                await _serviceBusPublisher.PublishMessageAsync("notifications", notification);
                Console.WriteLine($"Notification '{subject}' sent to '{recipientEmail}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing notification '{subject}': {ex.Message}");
            }
        }

        private async Task PublishSubscriptionEvent(string subject, string tenantName, string email, string subscriptionStatus)
        {
            try
            {
                var notification = new NotificationMessage
                {
                    Subject = subject,
                    RecipientEmail = email,
                    Payload = new NotificationPayload
                    {
                        TenantName = tenantName ?? "Unknown Tenant",
                        SubscriptionStatus = subscriptionStatus ?? "Unknown Status",
                        Message = $"Your subscription status is now {subscriptionStatus ?? "Unknown"}."
                    }
                };

                await _serviceBusPublisher.PublishMessageAsync("subscription-events", notification);
                Console.WriteLine($"Subscription event '{subject}' published for tenant '{tenantName}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing subscription event '{subject}': {ex.Message}");
            }
        }
    }
}
