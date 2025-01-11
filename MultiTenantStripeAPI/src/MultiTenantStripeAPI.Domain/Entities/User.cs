using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Domain.Entities
{
    public class User : BaseEntity
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty; // Links the user to the tenant
        public Tenant Tenant { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } // Navigation property
    }
}
