using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MultiTenantStripeAPI.Data;
using MultiTenantStripeAPI.Models;
using MultiTenantStripeAPI.Services.interfaces;
using MultiTenantStripeAPI.Services.Interfaces;
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

        private readonly IStripeService _stripeService;
        private readonly ITenantService _tenantService;
        private readonly IKeycloakService _keycloakService;

        public PaymentController(TenantDbContext context, IOptions<StripeOptions> stripeOptions, IStripeService stripeService, ITenantService tenantService, IKeycloakService keycloakService)
        {
            _context = context;
            _stripeOptions = stripeOptions.Value;
            _stripeService = stripeService;
            _tenantService = tenantService;
            _keycloakService = keycloakService;
        }

        [HttpPost("create-checkout-session")]
        public IActionResult CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            if (string.IsNullOrEmpty(request.TenantName) || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Tenant name and email are required.");
            }

            var session = _stripeService.CreateCheckoutSession(request.TenantName, request.Email);
            _tenantService.CreateTenant(request.TenantName, request.Email);

            return Ok(new { sessionId = session.Id });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var signatureHeader = Request.Headers["Stripe-Signature"];
                if (string.IsNullOrEmpty(signatureHeader))
                {
                    Console.WriteLine("Stripe-Signature header is missing.");
                    return BadRequest("Missing Stripe-Signature header.");
                }

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signatureHeader,
                    _stripeOptions.WebhookSecret,
                    throwOnApiVersionMismatch: false
                );

                Console.WriteLine("Stripe Event Type: " + stripeEvent.Type);

                // Handle supported events
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
                    case "product.created":
                    case "price.created":
                    case "charge.succeeded":
                    case "payment_intent.created":
                    case "payment_intent.succeeded":
                        Console.WriteLine($"Event {stripeEvent.Type} received but no specific handler is implemented.");
                        break;
                    default:
                        Console.WriteLine($"Unhandled Stripe event type: {stripeEvent.Type}");
                        break;
                }

                return Ok();
            }
            catch (StripeException e)
            {
                Console.WriteLine($"Stripe Exception: {e.Message}");
                return BadRequest($"Stripe Error: {e.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }





    }
}
