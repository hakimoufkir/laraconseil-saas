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
                await _emailService.SendEmailAsync(request.RecipientEmail, request.Subject, request.Body);
                return Ok("Email sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to send email: {ex.Message}");
            }
        }

         [HttpPost("subscription-update")]
        public async Task<IActionResult> SendSubscriptionUpdateEmail([FromBody] SubscriptionUpdateRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.SubscriptionStatus))
            {
                return BadRequest("Email and subscription status are required.");
            }

            var subject = "Your Subscription Update";
            var body = $@"
                <h1>Subscription Update</h1>
                <p>Hello,</p>
                <p>Your subscription status has been updated to: <strong>{request.SubscriptionStatus}</strong>.</p>
                <p>If you have any questions, feel free to contact us.</p>";

            await _emailService.SendEmailAsync(request.Email, subject, body);

            return Ok("Subscription update email sent.");
        }

        [HttpPost("realm-creation")]
        public async Task<IActionResult> SendRealmCreationEmail([FromBody] RealmCreationRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.RealmName) || string.IsNullOrEmpty(request.PortalLink))
            {
                return BadRequest("Email, realm name, and portal link are required.");
            }

            var subject = "Your Account Has Been Created";
            var body = $@"
                <h1>Welcome to Our Platform!</h1>
                <p>Hello,</p>
                <p>Your account has been successfully created under the realm: <strong>{request.RealmName}</strong>.</p>
                <p>You can access your portal using the following link:</p>
                <p><a href='{request.PortalLink}'>{request.PortalLink}</a></p>
                <p>Use your registered email address and the temporary password sent earlier to log in.</p>
                <p>If you encounter any issues, don't hesitate to reach out to our support team.</p>";

            await _emailService.SendEmailAsync(request.Email, subject, body);

            return Ok("Realm creation email sent.");
        }
    }
}
