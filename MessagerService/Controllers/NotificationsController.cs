using Microsoft.AspNetCore.Mvc;
using MessagerService.Models;
using MessagerService.Services;

namespace MessagerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly EmailService _emailService;

        public NotificationsController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid email request.");

            try
            {
                var senderEmail = "noreply@yourdomain.com"; // Use your sender email
                await _emailService.SendEmailAsync(request.RecipientEmail, request.Subject, request.Body);
                return Ok("Email sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to send email: {ex.Message}");
            }
        }
    }
}
