namespace MultiTenantStripeAPI.Application.IServices
{
    public interface IServiceBusPublisher
    {
        Task PublishMessageAsync(string topicName, string message);
    }
}