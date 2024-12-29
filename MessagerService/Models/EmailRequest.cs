using System.ComponentModel.DataAnnotations;

namespace MessagerService.Models
{
    public class EmailRequest
    {
        [Required]
        public string RecipientEmail { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }
    }
}
