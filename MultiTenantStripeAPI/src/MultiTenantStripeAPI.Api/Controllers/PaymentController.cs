using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MultiTenantStripeAPI.Application.Features.Payment.Commands;
using MultiTenantStripeAPI.Application.Features.Payment.Commands.CreateCheckoutSession;
using MultiTenantStripeAPI.Application.Features.Payment.Queries;
using MultiTenantStripeAPI.Domain.Entities;
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
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var signatureHeader = Request.Headers["Stripe-Signature"];

                if (string.IsNullOrEmpty(signatureHeader))
                {
                    return BadRequest("Missing Stripe-Signature header.");
                }

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signatureHeader,
                    _stripeOptions.WebhookSecret,
                    throwOnApiVersionMismatch: false
                );

                await _mediator.Send(new ProcessStripeEventCommand(stripeEvent));
                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest($"Stripe Error: {e.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
