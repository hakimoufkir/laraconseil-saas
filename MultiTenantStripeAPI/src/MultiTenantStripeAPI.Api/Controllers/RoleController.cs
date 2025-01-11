using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiTenantStripeAPI.Application.Features.Roles.Commands.AssignRole;
using MultiTenantStripeAPI.Application.Features.Roles.Queries.CheckPermission;

namespace MultiTenantStripeAPI.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RoleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return Ok(new { Success = result });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("check-permission")]
        public async Task<IActionResult> CheckPermission([FromQuery] int userId, [FromQuery] string permissionName)
        {
            try
            {
                var result = await _mediator.Send(new CheckPermissionQuery { UserId = userId, PermissionName = permissionName });
                return Ok(new { HasPermission = result });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
