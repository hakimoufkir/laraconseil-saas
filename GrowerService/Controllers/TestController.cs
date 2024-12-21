using Microsoft.AspNetCore.Mvc;

namespace GrowerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        // Health check endpoint
        [HttpGet("health")]
        public IActionResult Health()
        {
            try
            {
                return Ok("Healthy");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        // Test endpoint
        [HttpGet("message")]
        public IActionResult GetMessage()
        {
            try
            {
                return Ok("This is a test endpoint! grower-service");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }
    }
}
