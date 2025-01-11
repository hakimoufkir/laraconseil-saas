using Microsoft.OpenApi.Models;

namespace MultiTenantStripeAPI.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services, IConfiguration configuration)
        {
            var authorizationUrl = configuration["Keycloak:AuthorizationUrl"];
            if (string.IsNullOrEmpty(authorizationUrl))
            {
                throw new ArgumentNullException(nameof(authorizationUrl), "Keycloak Authorization URL is not configured.");
            }

            services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));
                options.AddSecurityDefinition("keycloak", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(authorizationUrl),
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "openid" },
                                { "profile", "profile" }
                            }
                        }
                    }
                });

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "keycloak",
                                Type = ReferenceType.SecurityScheme
                            },
                            In = ParameterLocation.Header,
                            Name = "Bearer",
                            Scheme = "Bearer"
                        },
                        Array.Empty<string>()
                    }
                };

                options.AddSecurityRequirement(securityRequirement);
            });

            return services;
        }
    }
}
