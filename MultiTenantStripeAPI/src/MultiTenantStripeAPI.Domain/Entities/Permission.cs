namespace MultiTenantStripeAPI.Domain.Entities
{
    public class Permission : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}