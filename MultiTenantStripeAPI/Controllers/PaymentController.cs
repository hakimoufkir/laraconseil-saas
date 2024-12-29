using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MultiTenantStripeAPI.Data;
using MultiTenantStripeAPI.Models;
using Stripe;
using Stripe.Checkout;
using Stripe.Events;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly TenantDbContext _context;
        private readonly StripeOptions _stripeOptions;

        public PaymentController(TenantDbContext context, IOptions<StripeOptions> stripeOptions)
        {
            _context = context;
            _stripeOptions = stripeOptions.Value;
        }

        [HttpPost("create-checkout-session")]
        public IActionResult CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            // Validate the input
            if (string.IsNullOrEmpty(request.TenantName) || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Tenant name and email are required.");
            }

            // Stripe session creation options for subscriptions
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                Price = "price_1Qb9QMIQYGMLN4GqsyWZd0VH", // Your price ID from Stripe Dashboard
                Quantity = 1,
            },
        },
                Mode = "subscription", // Use subscription mode
                SuccessUrl = "http://localhost:4200/success",
                CancelUrl = "http://localhost:4200/cancel",
            };

            var service = new SessionService();
            var session = service.Create(options);

            // Save tenant to DB with "Pending" status
            var tenant = new Tenant
            {
                TenantName = request.TenantName,
                Email = request.Email,
                TenantId = Guid.NewGuid().ToString(),
                SubscriptionStatus = "Pending"
            };

            _context.Tenants.Add(tenant);
            _context.SaveChanges();

            return Ok(new { sessionId = session.Id });
        }



        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                Console.WriteLine("Raw JSON Payload: " + json);

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _stripeOptions.WebhookSecret,
                    throwOnApiVersionMismatch: false
                );

                Console.WriteLine("Stripe Event Type: " + stripeEvent.Type);

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    var customerEmail = session?.CustomerDetails?.Email;

                    Console.WriteLine("Checkout Session Completed for: " + customerEmail);

                    var tenant = _context.Tenants.FirstOrDefault(t => t.Email == customerEmail);
                    if (tenant != null)
                    {
                        tenant.SubscriptionStatus = "Active";
                        _context.SaveChanges();
                        Console.WriteLine("Tenant subscription updated to Active.");
                    }
                    else
                    {
                        Console.WriteLine("Tenant not found for email: " + customerEmail);
                    }
                }
                else if (stripeEvent.Type == "customer.subscription.updated")
                {
                    var subscription = stripeEvent.Data.Object as Subscription;
                    var customer = await new CustomerService().GetAsync(subscription.CustomerId);
                    var customerEmail = customer.Email;

                    Console.WriteLine($"Subscription updated for: {customerEmail}, Status: {subscription.Status}");

                    var tenant = _context.Tenants.FirstOrDefault(t => t.Email == customerEmail);
                    if (tenant != null)
                    {
                        tenant.SubscriptionStatus = subscription.Status; // "active", "canceled", etc.
                        _context.SaveChanges();
                    }
                }
                else if (stripeEvent.Type == "customer.subscription.deleted")
                {
                    var subscription = stripeEvent.Data.Object as Subscription;
                    var customer = await new CustomerService().GetAsync(subscription.CustomerId);
                    var customerEmail = customer.Email;

                    Console.WriteLine($"Subscription deleted for: {customerEmail}");

                    var tenant = _context.Tenants.FirstOrDefault(t => t.Email == customerEmail);
                    if (tenant != null)
                    {
                        tenant.SubscriptionStatus = "Canceled";
                        _context.SaveChanges();
                    }
                }

                return Ok();
            }
            catch (StripeException e)
            {
                Console.WriteLine("Stripe Exception: " + e.Message);
                return BadRequest(e.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("General Exception: " + ex.Message);
                return BadRequest(ex.Message);
            }
        }



    }
}
