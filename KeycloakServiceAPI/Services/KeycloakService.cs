using System.Net.Http.Headers;
using System.Text.Json;
using KeycloakServiceAPI.Models;

namespace KeycloakServiceAPI.Services;

public class KeycloakService
{
    private readonly HttpClient _httpClient;
    private readonly ServiceBusPublisher _serviceBusPublisher;

    public KeycloakService(HttpClient httpClient, ServiceBusPublisher serviceBusPublisher)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _serviceBusPublisher = serviceBusPublisher ?? throw new ArgumentNullException(nameof(serviceBusPublisher));
    }

    public async Task CreateRealmAndUserAsync(string realmName, string tenantEmail)
    {
        Console.WriteLine($"Creating realm '{realmName}' and user '{tenantEmail}' in Keycloak...");

        try
        {
            // Step 1: Retrieve Admin Token
            var adminToken = await GetAdminTokenAsync();

            // Step 2: Create Realm
            await CreateRealmAsync(realmName, adminToken);

            // Step 3: Create User
            await CreateUserAsync(realmName, tenantEmail, adminToken);

            // Publish success notification
            await _serviceBusPublisher.PublishNotificationAsync(
                "Realm Created",
                tenantEmail,
                $"Your realm '{realmName}' has been successfully created."
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in KeycloakService.CreateRealmAndUserAsync: {ex.Message}");
            throw;
        }
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", "admin-cli"),
            new KeyValuePair<string, string>("username", "admin"),
            new KeyValuePair<string, string>("password", "StrongPassword@123"),
            new KeyValuePair<string, string>("grant_type", "password"),
        });

        var response = await _httpClient.PostAsync("https://keycloaklaraconseil.azurewebsites.net/realms/master/protocol/openid-connect/token", form);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(jsonResponse);

        return tokenResponse?.AccessToken ?? throw new Exception("Failed to retrieve Keycloak admin token.");
    }

    private async Task CreateRealmAsync(string realmName, string adminToken)
    {
        var realm = new { id = realmName, realm = realmName, enabled = true };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://keycloaklaraconseil.azurewebsites.net/admin/realms")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", adminToken) },
            Content = new StringContent(JsonSerializer.Serialize(realm), System.Text.Encoding.UTF8, "application/json"),
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        Console.WriteLine($"Realm '{realmName}' created successfully.");
    }

    private async Task CreateUserAsync(string realmName, string tenantEmail, string adminToken)
    {
        var user = new
        {
            username = tenantEmail,
            email = tenantEmail,
            enabled = true,
            credentials = new[] { new { type = "password", value = "SecurePassword123", temporary = false } }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"https://keycloaklaraconseil.azurewebsites.net/admin/realms/{realmName}/users")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", adminToken) },
            Content = new StringContent(JsonSerializer.Serialize(user), System.Text.Encoding.UTF8, "application/json"),
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        Console.WriteLine($"User '{tenantEmail}' created successfully in realm '{realmName}'.");
    }
}
