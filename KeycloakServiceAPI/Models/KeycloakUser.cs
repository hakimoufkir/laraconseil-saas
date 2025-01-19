using System.Text.Json.Serialization;

namespace KeycloakServiceAPI.Models
{
    public class KeycloakUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("createdTimestamp")]
        public long CreatedTimestamp { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("totp")]
        public bool Totp { get; set; }

        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("disableableCredentialTypes")]
        public List<string> DisableableCredentialTypes { get; set; }

        [JsonPropertyName("requiredActions")]
        public List<string> RequiredActions { get; set; }

        [JsonPropertyName("notBefore")]
        public int NotBefore { get; set; }

        [JsonPropertyName("access")]
        public Access Access { get; set; }
    }

    public class Access
    {
        [JsonPropertyName("manageGroupMembership")]
        public bool ManageGroupMembership { get; set; }

        [JsonPropertyName("view")]
        public bool View { get; set; }

        [JsonPropertyName("mapRoles")]
        public bool MapRoles { get; set; }

        [JsonPropertyName("impersonate")]
        public bool Impersonate { get; set; }

        [JsonPropertyName("manage")]
        public bool Manage { get; set; }
    }
}