using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using MultiTenantStripeAPI.Application.Features.Tenant.Commands.CreateTenant;
using MultiTenantStripeAPI.Application.Features.Tenant.Queries.GetTenantByEmail;

namespace MultiTenantStripeAPI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TenantController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command)
        {
            if (command == null || string.IsNullOrEmpty(command.TenantName) || string.IsNullOrEmpty(command.Email))
            {
                return BadRequest("Tenant name and email are required.");
            }

            try
            {
                var tenantId = await _mediator.Send(command);
                return Ok(new { Message = "Tenant created successfully.", TenantId = tenantId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("get-by-email")]
        public async Task<IActionResult> GetTenantByEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required.");
            }

            try
            {
                var tenant = await _mediator.Send(new GetTenantByEmailQuery { Email = email });
                return Ok(tenant);
            }
            catch (Exception ex)
            {
                return NotFound($"An error occurred: {ex.Message}");
            }
        }
    }
}
