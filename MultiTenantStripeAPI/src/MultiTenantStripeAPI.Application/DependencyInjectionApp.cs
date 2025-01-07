using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using MultiTenantStripeAPI.Application.IServices;
using MultiTenantStripeAPI.Application.Services;
using Microsoft.Extensions.Configuration;

namespace MultiTenantStripeAPI.Application
{
    public static class DependencyInjectionApp
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Application Layer Services
            services.AddTransient<IStripeService, StripeService>();
            services.AddTransient<IKeycloakService, KeycloakService>();
            services.AddTransient<ITenantService, TenantService>();

            // Register ServiceBusPublisher
            var serviceBusConnectionString = configuration["ConnectionStrings:AzureServiceBus"]
            ?? throw new InvalidOperationException("Azure Service Bus connection string is not set in the configuration.");
            services.AddTransient<IServiceBusPublisher>(_ => new ServiceBusPublisher(serviceBusConnectionString));


            // Register MediatR for CQRS
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
