using Stripe;
using Stripe.Checkout;
using System.Threading.Tasks;

namespace MultiTenantStripeAPI.Application.IServices
{
    public interface IStripeService
    {
        /// <summary>
        /// Creates a Stripe checkout session for the given tenant and plan type.
        /// </summary>
        /// <param name="tenantName">The name of the tenant.</param>
        /// <param name="email">The email of the tenant's owner.</param>
        /// <param name="planType">The subscription plan type (e.g., "Grower", "Station").</param>
        /// <returns>The created Stripe session.</returns>
        Session CreateCheckoutSession(string tenantName, string email, string planType, string tenantId);

        /// <summary>
        /// Handles the Stripe event for a completed checkout session.
        /// </summary>
        /// <param name="stripeEvent">The Stripe event data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandleCheckoutSessionCompleted(Event stripeEvent);

        /// <summary>
        /// Handles the Stripe event for an updated subscription.
        /// </summary>
        /// <param name="stripeEvent">The Stripe event data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandleSubscriptionUpdated(Event stripeEvent);

        /// <summary>
        /// Handles the Stripe event for a deleted subscription.
        /// </summary>
        /// <param name="stripeEvent">The Stripe event data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandleSubscriptionDeleted(Event stripeEvent);
    }
}
