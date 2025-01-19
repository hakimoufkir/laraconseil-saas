using System.Net.Http.Headers;
using System.Text.Json;
using KeycloakServiceAPI.Models;

namespace KeycloakServiceAPI.Services
{
    public class KeycloakService
    {
        private readonly HttpClient _httpClient;
        private readonly ServiceBusPublisher _serviceBusPublisher;

        public KeycloakService(HttpClient httpClient, ServiceBusPublisher serviceBusPublisher)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _serviceBusPublisher = serviceBusPublisher ?? throw new ArgumentNullException(nameof(serviceBusPublisher));
        }

        // Stage 2: Assign roles to user after payment
        public async Task AssignRolesToUserAsync(string tenantEmail, string planType)
        {
            try
            {
                // Step 1: Retrieve Admin Token
                var adminToken = await GetAdminTokenAsync();
                if (string.IsNullOrEmpty(adminToken))
                {
                    Console.WriteLine("[Error] Admin token retrieval failed.");
                    return;
                }

                // Step 3: Assign roles to the user
                await AssignRolesToUserInKeycloakAsync(tenantEmail, planType, adminToken);

                // Publish success notification
                await _serviceBusPublisher.PublishNotificationAsync(
                    "Roles Assigned to User",
                    tenantEmail,
                    $"Welcome to LaraConseil! Your account has been successfully created. To access all the features of our platform, please complete the payment process. Once your payment is confirmed, you'll have access to the full application and be assigned the appropriate roles.\n\n" +
                    "You can check your portal using the following credentials:\n\n" +
                $"Email: {tenantEmail}\nPassword: SecurePassword@123\n\nEnjoy exploring the platform and all its features!"                    
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Error in KeycloakService.AssignRolesToUserAsync: {ex.Message}");
                throw;
            }
        }

        // Stage 1: Register a user without roles
        public async Task CreateUserWithoutRolesAsync(string tenantName, string tenantEmail)
        {
            try
            {
                // Step 1: Retrieve Admin Token
                var adminToken = await GetAdminTokenAsync();
                if (string.IsNullOrEmpty(adminToken))
                {
                    Console.WriteLine("[Error] Admin token retrieval failed.");
                    return;
                }

                // Step 3: Create the user without roles
                await CreateUserInKeycloakWithoutRolesAsync(tenantName, tenantEmail, adminToken);

                // Publish success notification
                await _serviceBusPublisher.PublishNotificationAsync(
                    "User Created",
                    tenantEmail,
                $"Congratulations! You now have full access to your application on LaraConseil. You can check your portal using the following this link:" +
                "https://webapp.gentlegrass-3889baac.westeurope.azurecontainerapps.io/dashboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Error in KeycloakService.CreateUserWithoutRolesAsync: {ex.Message}");
                throw;
            }
        }

        // Step 1: Retrieve Admin Token
        private async Task<string> GetAdminTokenAsync()
        {
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "admin-cli"),
                new KeyValuePair<string, string>("username", "admin"),
                new KeyValuePair<string, string>("password", "StrongPassword@123"),
                new KeyValuePair<string, string>("grant_type", "password"),
            });

            try
            {
                var response = await _httpClient.PostAsync("https://keycloaklaraconseil.azurewebsites.net/realms/master/protocol/openid-connect/token", form);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(jsonResponse);

                return tokenResponse?.AccessToken ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to retrieve admin token: {ex.Message}");
                return string.Empty;
            }
        }

        // Create user without roles in Keycloak
        private async Task CreateUserInKeycloakWithoutRolesAsync(string tenantName, string tenantEmail, string adminToken)
        {
            try
            {
                var user = new
                {
                    username = tenantName,
                    email = tenantEmail,
                    enabled = true,
                    credentials = new[] { new { type = "password", value = "SecurePassword@123", temporary = true } },
                    requiredActions = new[] { "VERIFY_EMAIL" }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, $"https://keycloaklaraconseil.azurewebsites.net/admin/realms/MultiTenantRealm/users")
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", adminToken) },
                    Content = new StringContent(JsonSerializer.Serialize(user), System.Text.Encoding.UTF8, "application/json"),
                };

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[Info] User '{tenantEmail}' created successfully in realm '{tenantName}' without roles.");
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Error] Error creating user: {responseContent}");
                    throw new Exception($"Failed to create user: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Error in creating user: {ex.Message}");
                throw;
            }
        }

        // Assign roles to a user in Keycloak (Realm Roles)
        private async Task AssignRolesToUserInKeycloakAsync(string tenantEmail, string planType, string adminToken)
        {
            try
            {
                // Step 1: Get all roles in the realm
                var roles = await GetAllRolesAsync(adminToken);

                if (roles == null || roles.Count == 0)
                {
                    Console.WriteLine("[Error] No roles found in the realm.");
                    return;
                }

                // Step 2: Select the role based on planType
                var roleName = planType == "Grower" ? "Grower" : "Station";
                var role = roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));

                if (role == null)
                {
                    Console.WriteLine($"[Error] Role '{roleName}' not found.");
                    return;
                }

                // Step 3: Get the user ID using the email address
                var userId = await GetUserIdByEmailAsync(tenantEmail, adminToken);

                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine($"[Error] User '{tenantEmail}' not found. Cannot assign roles.");
                    return;
                }

                // Step 4: Assign the selected role to the user
                var roleMapping = new[]
                {
            new
            {
                id = role.Id,
                name = role.Name,
                description = role.Description ?? $"{role.Name} role"
            }
        };

                var request = new HttpRequestMessage(HttpMethod.Post, $"https://keycloaklaraconseil.azurewebsites.net/admin/realms/MultiTenantRealm/users/{userId}/role-mappings/realm")
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", adminToken) },
                    Content = new StringContent(JsonSerializer.Serialize(roleMapping), System.Text.Encoding.UTF8, "application/json")
                };

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[Info] Roles '{roleName}' assigned successfully to user '{tenantEmail}'.");
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Error] Error assigning roles to user: {responseContent}");
                    throw new Exception($"Failed to assign roles: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Error in assigning roles: {ex.Message}");
                throw;
            }
        }

        // Get all roles in the realm
        private async Task<List<KeycloakRole>> GetAllRolesAsync(string adminToken)
        {
            var url = "https://keycloaklaraconseil.azurewebsites.net/admin/realms/MultiTenantRealm/roles";
            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", adminToken) }
            };

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Debug] Role lookup response content: {content}");  // Added log here
                var roles = JsonSerializer.Deserialize<List<KeycloakRole>>(content);
                return roles;
            }

            Console.WriteLine($"[Error] Failed to retrieve roles. Response status code: {response.StatusCode}");  // Added log here
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[Debug] Error response: {responseContent}");  // Added log here
            return null;
        }

        // Get the user ID by email address
        private async Task<string> GetUserIdByEmailAsync(string tenantEmail, string adminToken)
        {
            var url = $"https://keycloaklaraconseil.azurewebsites.net/admin/realms/MultiTenantRealm/users?email={tenantEmail}";

            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", adminToken) }
            };

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Debug] User lookup response content: {content}");  // Added log here
                var users = JsonSerializer.Deserialize<List<KeycloakUser>>(content);

                // Return the user ID if found
                return users != null && users.Count > 0 ? users[0].Id : null;
            }

            Console.WriteLine($"[Error] User with email '{tenantEmail}' not found. Response status code: {response.StatusCode}");  // Added log here
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[Debug] Error response: {responseContent}");  // Added log here
            return null;
        }
    }
}