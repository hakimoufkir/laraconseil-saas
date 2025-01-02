using Microsoft.Extensions.Options;
using MultiTenantStripeAPI.Application.IServices;
using MultiTenantStripeAPI.Domain.Entities;
using Stripe;
using Stripe.Checkout;
using System.Text.Json;

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
            _stripeOptions = stripeOptions.Value ?? throw new ArgumentNullException(nameof(stripeOptions));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _keycloakService = keycloakService ?? throw new ArgumentNullException(nameof(keycloakService));
            _serviceBusPublisher = serviceBusPublisher ?? throw new ArgumentNullException(nameof(serviceBusPublisher));
            StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
        }

        public Session CreateCheckoutSession(string tenantName, string email)
        {
            Console.WriteLine($"Creating checkout session for tenant: {tenantName}, email: {email}");

            // Create or retrieve a customer in Stripe
            var customerService = new CustomerService();
            var existingCustomer = customerService.List(new CustomerListOptions { Email = email }).Data.FirstOrDefault();

            // If the customer does not exist, create a new one
            var customerId = existingCustomer?.Id ?? customerService.Create(new CustomerCreateOptions { Email = email }).Id;

            if (string.IsNullOrEmpty(customerId))
            {
                // If customer creation or retrieval fails, throw an exception
                throw new InvalidOperationException("Failed to create or retrieve customer ID.");
            }

            // Define the options for creating the Stripe checkout session
            var options = new SessionCreateOptions
            {
                Customer = customerId, // Associate the session with the customer
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                Price = "price_1Qb9QMIQYGMLN4GqsyWZd0VH", // Stripe price ID
                Quantity = 1, // Quantity of the subscription
            },
        },
                Mode = "subscription", // Specify subscription mode
                SuccessUrl = "http://localhost:4200/success", // Redirect URL on success
                CancelUrl = "http://localhost:4200/cancel", // Redirect URL on cancellation
            };

            // Create the session using Stripe's SessionService
            var service = new SessionService();
            var session = service.Create(options);

            Console.WriteLine($"Session created successfully: {session.Id}");
            return session;
        }

        public async Task HandleCheckoutSessionCompleted(Event stripeEvent)
        {
            if (stripeEvent?.Data?.Object is not Session session)
            {
                // If the session object is missing or invalid, log and return
                Console.WriteLine("Stripe session is null or invalid.");
                return;
            }

            // Extract customer details from the session
            string? customerId = session.CustomerId;
            string? customerEmail = session.CustomerDetails?.Email;

            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                // If the customer email is missing, log and return
                Console.WriteLine("Customer email is missing. Cannot proceed.");
                return;
            }

            Tenant tenant;

            try
            {
                // Attempt to retrieve the tenant by email
                tenant = _tenantService.GetTenantByEmail(customerEmail);
                Console.WriteLine($"Tenant found for email: {customerEmail}");
            }
            catch (InvalidOperationException)
            {
                // If the tenant does not exist, create a new one
                Console.WriteLine($"Tenant not found for email '{customerEmail}'. Creating a new tenant.");
                string tenantName = session.CustomerDetails?.Name ?? "Unknown Tenant";
                tenant = _tenantService.CreateTenant(tenantName, customerEmail);
            }

            try
            {
                // Update the tenant's subscription status to Active
                _tenantService.UpdateTenantStatus(tenant, "Active");
                Console.WriteLine("Tenant subscription status updated to Active.");

                // Notify the customer about the subscription activation
                await SendNotificationAsync("Subscription Activated", tenant.Email, new NotificationPayload
                {
                    TenantName = tenant.TenantName,
                    SubscriptionStatus = tenant.SubscriptionStatus,
                    Message = "Your subscription has been activated."
                });
            }
            catch (Exception ex)
            {
                // Log any errors encountered while updating the tenant's status
                Console.WriteLine($"Error updating subscription status for tenant '{tenant.TenantName}': {ex.Message}");
            }

            try
            {
                // Create a Keycloak realm and user for the tenant
                await _keycloakService.CreateRealmAndUserAsync(tenant.TenantName, tenant.Email);
                Console.WriteLine("Keycloak realm and user created successfully.");

                // Notify the customer about the realm creation
                await SendNotificationAsync("Realm Created", tenant.Email, new NotificationPayload
                {
                    TenantName = tenant.TenantName,
                    Realm = tenant.TenantName, // Assuming 'Realm' is a valid property in NotificationPayload
                    Message = "Your realm has been successfully created. Please check your email for login details."
                });
            }
            catch (Exception ex)
            {
                // Log any errors encountered while creating the Keycloak realm
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
            await SendNotificationAsync("Subscription Updated", tenant.Email, new NotificationPayload
            {
                TenantName = tenant.TenantName,
                SubscriptionStatus = subscription.Status, // Assuming 'SubscriptionStatus' is a valid property in NotificationPayload
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
            await SendNotificationAsync("Subscription Canceled", tenant.Email, new NotificationPayload
            {
                TenantName = tenant.TenantName,
                Message = "Your subscription has been canceled."
            });
        }

        private async Task SendNotificationAsync(string subject, string recipientEmail, NotificationPayload payload)
        {
            var notificationMessage = new NotificationMessage
            {
                Subject = subject,
                RecipientEmail = recipientEmail,
                Payload = payload
            };

            string message = JsonSerializer.Serialize(notificationMessage);
            Console.WriteLine($"message sent: {JsonSerializer.Serialize(notificationMessage)}");


            await _serviceBusPublisher.PublishMessageAsync("notifications", message);

            Console.WriteLine($"Notification sent: {message}");
        }


    }
}
