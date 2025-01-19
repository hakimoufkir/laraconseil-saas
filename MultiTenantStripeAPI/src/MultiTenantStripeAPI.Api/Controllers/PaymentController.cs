using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MultiTenantStripeAPI.Application.Features.Payment.Commands;
using MultiTenantStripeAPI.Application.Features.Payment.Commands.CreateCheckoutSession;
using MultiTenantStripeAPI.Application.Features.Payment.Commands.ProcessStripeEvent;
using MultiTenantStripeAPI.Application.Features.Payment.Queries;
using MultiTenantStripeAPI.Domain.Entities;
using Newtonsoft.Json;
using Stripe;
using Stripe.Events;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly StripeOptions _stripeOptions;

        public PaymentController(IMediator mediator, IOptions<StripeOptions> stripeOptions)
        {
            _mediator = mediator;
            _stripeOptions = stripeOptions.Value;
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionCommand command)
        {
            if (command == null || string.IsNullOrEmpty(command.TenantName) || string.IsNullOrEmpty(command.Email))
            {
                return BadRequest("Tenant name and email are required.");
            }

            try
            {
                Console.WriteLine($"Received TenantName: {command.TenantName}, Email: {command.Email}");
                var result = await _mediator.Send(command);
                return Ok(new { sessionId = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            try
            {
                // Read the raw JSON data from the request body
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

                // Retrieve the Stripe signature header
                var signatureHeader = Request.Headers["Stripe-Signature"];

                // Check for missing signature
                if (string.IsNullOrEmpty(signatureHeader))
                {
                    Console.WriteLine("Missing Stripe-Signature header.");
                    return BadRequest("Missing Stripe-Signature header.");
                }

                Console.WriteLine($"Received Stripe webhook event with signature: {signatureHeader}");

                // Construct the Stripe event from the incoming JSON and signature
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signatureHeader,
                    _stripeOptions.WebhookSecret,
                    throwOnApiVersionMismatch: false
                );

                // Log the entire stripeEvent to the console for debugging
                var stripeEventJson = JsonConvert.SerializeObject(stripeEvent, Formatting.Indented);
                //Console.WriteLine($"Stripe Event: {stripeEventJson}");

                // Process the Stripe event (pass the event to your mediator handler)
                await _mediator.Send(new ProcessStripeEventCommand(stripeEvent));

                // Log success message
                Console.WriteLine("Stripe event processed successfully.");

                return Ok();
            }
            catch (StripeException e)
            {
                Console.WriteLine($"Stripe Error while processing webhook: {e.Message}");
                return BadRequest($"Stripe Error: {e.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while processing the webhook: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


    }
}
