using System.Net.Http.Headers;
using System.Text.Json;
using MultiTenantStripeAPI.Models;
using MultiTenantStripeAPI.Services.interfaces;

namespace MultiTenantStripeAPI.Services
{
    public class KeycloakService : IKeycloakService
    {
        private readonly HttpClient _httpClient;

        public KeycloakService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetAdminTokenAsync()
        {
            const int maxRetries = 3;
            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    attempt++;

                    var form = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("client_id", "admin-cli"),
                        new KeyValuePair<string, string>("username", "admin"),
                        new KeyValuePair<string, string>("password", "admin"),
                        new KeyValuePair<string, string>("grant_type", "password")
                    });

                    var response = await _httpClient.PostAsync("http://keycloak:8080/realms/master/protocol/openid-connect/token", form);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to get admin token: {response.ReasonPhrase}");
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(jsonResponse);

                    if (string.IsNullOrEmpty(tokenResponse?.AccessToken))
                    {
                        throw new Exception("Access token not found in Keycloak response.");
                    }

                    return tokenResponse.AccessToken;
                }
                catch (Exception ex)
                {
                    if (attempt >= maxRetries)
                    {
                        throw new Exception($"Failed to get admin token after {maxRetries} attempts: {ex.Message}");
                    }

                    Console.WriteLine($"Retrying to get admin token ({attempt}/{maxRetries})...");
                    await Task.Delay(1000);
                }
            }

            throw new Exception("Failed to get admin token: Unknown error.");
        }

        public async Task CreateRealmAndUserAsync(string realmName, string tenantEmail)
        {
            try
            {
                // Step 1: Retrieve Admin Token
                var token = await GetAdminTokenAsync();

                // Step 2: Create a Realm
                var realm = new
                {
                    id = realmName,
                    realm = realmName,
                    enabled = true
                };

                var realmRequest = new HttpRequestMessage(HttpMethod.Post, "http://keycloak:8080/admin/realms")
                {
                    Headers =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", token)
            },
                    Content = new StringContent(JsonSerializer.Serialize(realm), System.Text.Encoding.UTF8, "application/json")
                };

                var realmResponse = await _httpClient.SendAsync(realmRequest);
                if (!realmResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create realm: {realmResponse.ReasonPhrase} - {await realmResponse.Content.ReadAsStringAsync()}");
                }

                Console.WriteLine($"Realm '{realmName}' created successfully.");

                // Step 3: Create a User in the Realm
                var user = new
                {
                    username = tenantEmail,
                    email = tenantEmail,
                    enabled = true,
                    credentials = new[]
                    {
                new { type = "password", value = "SecurePassword123", temporary = false }
            }
                };

                var userRequest = new HttpRequestMessage(HttpMethod.Post, $"http://keycloak:8080/admin/realms/{realmName}/users")
                {
                    Headers =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", token)
            },
                    Content = new StringContent(JsonSerializer.Serialize(user), System.Text.Encoding.UTF8, "application/json")
                };

                var userResponse = await _httpClient.SendAsync(userRequest);
                if (!userResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create user in realm '{realmName}': {userResponse.ReasonPhrase} - {await userResponse.Content.ReadAsStringAsync()}");
                }

                Console.WriteLine($"User '{tenantEmail}' created successfully in realm '{realmName}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateRealmAndUserAsync: {ex.Message}");
                throw;
            }
        }
    }
}
