using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiTenantStripeAPI.Application.Features.Users.Commands.CreateUser;
using MultiTenantStripeAPI.Application.IServices;
using MultiTenantStripeAPI.Application.Services;
using MultiTenantStripeAPI.Domain.Entities;

namespace MultiTenantStripeAPI.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IServiceBusPublisher _serviceBusPublisher;

        public UserController(IMediator mediator, IServiceBusPublisher serviceBusPublisher)
        {
            _mediator = mediator;
            _serviceBusPublisher = serviceBusPublisher;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            try
            {
                int userId = await _mediator.Send(command);
                return Ok(new { Message = "User created successfully.", UserId = userId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("signup")]
        public async Task<IActionResult> CreateUserWithoutRolesAsync([FromBody] KeycloakActionData createUserWithoutRolesAsync)
        {
            try
            {
                await _serviceBusPublisher.PublishMessageAsync("keycloak-actions", new KeycloakActionMessage
                {
                    Action = "CreateUserWithoutRoles",
                    Data = new KeycloakActionData
                    {
                        TenantName = createUserWithoutRolesAsync.TenantName,
                        TenantEmail = createUserWithoutRolesAsync.TenantEmail
                    }
                });
                return Ok(new { Message = "User created successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }


    }
}
