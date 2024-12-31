using Microsoft.Extensions.Options;
using MultiTenantStripeAPI.Models;
using MultiTenantStripeAPI.Services.interfaces;
using MultiTenantStripeAPI.Services.Interfaces;
using Stripe;
using Stripe.Checkout;
using System.Text.Json;

namespace MultiTenantStripeAPI.Services
{
    public class StripeService : IStripeService
    {
        private readonly StripeOptions _stripeOptions;
        private readonly ITenantService _tenantService;
        private readonly IKeycloakService _keycloakService;
        private readonly ServiceBusPublisher _publisher;

        public StripeService(
            IOptions<StripeOptions> stripeOptions,
            ITenantService tenantService,
            IKeycloakService keycloakService,
            ServiceBusPublisher publisher)
        {
            _stripeOptions = stripeOptions.Value ?? throw new ArgumentNullException(nameof(stripeOptions));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _keycloakService = keycloakService ?? throw new ArgumentNullException(nameof(keycloakService));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
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

            string? customerId = session.CustomerId;
            string? customerEmail = session.CustomerDetails?.Email;

            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                Console.WriteLine("Customer email is missing. Cannot proceed.");
                return;
            }

            Tenant tenant;

            try
            {
                // Check if the tenant already exists
                tenant = _tenantService.GetTenantByEmail(customerEmail);
                Console.WriteLine($"Tenant found for email: {customerEmail}");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine($"No tenant found with email '{customerEmail}'. Creating a new tenant.");

                // Attempt to create the tenant
                string tenantName = session.CustomerDetails?.Name ?? "Unknown Tenant";
                try
                {
                    tenant = _tenantService.CreateTenant(tenantName, customerEmail);

                    // Notify the customer about tenant creation
                    await SendNotificationAsync("Tenant Created", tenant.Email, new
                    {
                        TenantName = tenant.TenantName,
                        Message = "Your tenant has been successfully created!"
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating tenant for email '{customerEmail}': {ex.Message}");
                    return;
                }
            }

            try
            {
                // Update subscription status
                _tenantService.UpdateTenantStatus(tenant, "Active");
                Console.WriteLine("Tenant subscription status updated to Active.");

                // Notify the customer about subscription status
                await SendNotificationAsync("Subscription Activated", tenant.Email, new
                {
                    TenantName = tenant.TenantName,
                    Message = "Your subscription has been activated."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating subscription status for tenant '{tenant.TenantName}': {ex.Message}");
            }

            try
            {
                // Create realm and user in Keycloak
                await _keycloakService.CreateRealmAndUserAsync(tenant.TenantName, tenant.Email);
                Console.WriteLine("Keycloak realm and user created successfully.");

                // Notify the customer about realm creation
                await SendNotificationAsync("Realm Created", tenant.Email, new
                {
                    TenantName = tenant.TenantName,
                    Realm = tenant.TenantName,
                    Message = "Your realm has been successfully created. Please check your email for login details."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Keycloak realm and user for tenant '{tenant.TenantName}': {ex.Message}");
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

            // Notify the customer about subscription update
            await SendNotificationAsync("Subscription Updated", tenant.Email, new
            {
                TenantName = tenant.TenantName,
                Status = subscription.Status,
                Message = $"Your subscription status has been updated to {subscription.Status}."
            });
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

            // Notify the customer about subscription cancellation
            await SendNotificationAsync("Subscription Canceled", tenant.Email, new
            {
                TenantName = tenant.TenantName,
                Message = "Your subscription has been canceled."
            });
        }

        private async Task SendNotificationAsync(string subject, string recipientEmail, object payload)
        {
            var notificationMessage = new
            {
                Subject = subject,
                RecipientEmail = recipientEmail,
                Payload = payload
            };

            string message = JsonSerializer.Serialize(notificationMessage);

            await _publisher.PublishMessageAsync("notifications", message);

            Console.WriteLine($"Notification sent: {message}");
        }
    }
}
