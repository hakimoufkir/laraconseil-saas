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

        // Station-specific endpoint
        [Authorize(Policy = "StationPolicy")]
        [HttpGet("station")]
        public IActionResult GetStationData()
        {
            return Ok("Station data accessed.");
        }


        // Get current user's claims
        [Authorize]
        [HttpGet("claims")]
        public IActionResult GetClaims()
        {
            var claims = HttpContext.User.Claims
                .GroupBy(c => c.Type)
                .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(c => c.Value)));

            return Ok(claims);
        }

    }
}
