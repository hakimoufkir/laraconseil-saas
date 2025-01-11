using MediatR;


namespace MultiTenantStripeAPI.Application.Features.Tenant.Queries.GetTenantByEmail
{
    public class GetTenantByEmailQuery : IRequest<MultiTenantStripeAPI.Domain.Entities.Tenant>
    {
        public string Email { get; set; } = string.Empty;
    }
}
