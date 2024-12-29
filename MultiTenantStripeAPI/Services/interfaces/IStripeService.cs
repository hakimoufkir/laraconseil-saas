using Stripe;
using Stripe.Checkout;

namespace MultiTenantStripeAPI.Services.Interfaces
{
    public interface IStripeService
    {
        Session CreateCheckoutSession(string tenantName, string email);
        Task HandleCheckoutSessionCompleted(Event stripeEvent);
        Task HandleSubscriptionUpdated(Event stripeEvent);
        Task HandleSubscriptionDeleted(Event stripeEvent);
    }
}
