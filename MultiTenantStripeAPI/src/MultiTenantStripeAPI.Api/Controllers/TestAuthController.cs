using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MultiTenantStripeAPI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestAuthController : ControllerBase
    {
        // Anonymous access endpoint
        [AllowAnonymous]
        [HttpGet("anonymous")]
        public IActionResult AnonymousEndpoint()
        {
            return Ok("This is an anonymous endpoint. No authentication required.");
        }

        // Grower-specific endpoint
        [Authorize(Policy = "GrowerPolicy")]
        [HttpGet("grower")]
        public IActionResult GrowerEndpoint()
        {
            return Ok("This is a Grower-specific endpoint. Access granted.");
        }

        // StationAdmin-specific endpoint
        [Authorize(Policy = "StationAdminPolicy")]
        [HttpGet("station-admin")]
        public IActionResult StationAdminEndpoint()
        {
            return Ok("This is a StationAdmin-specific endpoint. Access granted.");
        }

        [Authorize]
        [HttpGet("claims")]
        public IActionResult GetClaims(ClaimsPrincipal claimsPrincipal)
        {
            var claims = claimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
            return Ok(claims);
        }
    }
}
