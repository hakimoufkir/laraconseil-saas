using Azure.Communication.Email;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessagerService.Services
{
    public class EmailService
    {
        private readonly EmailClient _emailClient;
        private readonly string _senderEmail;

        public EmailService(string connectionString, string senderEmail)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrEmpty(senderEmail)) throw new ArgumentNullException(nameof(senderEmail));

            _emailClient = new EmailClient(connectionString);
            _senderEmail = senderEmail;
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(recipientEmail)) throw new ArgumentNullException(nameof(recipientEmail));
            if (string.IsNullOrEmpty(subject)) throw new ArgumentNullException(nameof(subject));
            if (string.IsNullOrEmpty(htmlBody)) throw new ArgumentNullException(nameof(htmlBody));

            var emailContent = new EmailContent(subject)
            {
                Html  = htmlBody
            };

            var emailMessage = new EmailMessage(_senderEmail, recipientEmail, emailContent);

            try
            {
                // Send the email
                var response = await _emailClient.SendAsync(Azure.WaitUntil.Completed, emailMessage, cancellationToken);
                Console.WriteLine($"Email sent successfully! response.Value: {response.Value}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                throw;
            }
        }
    }
}
