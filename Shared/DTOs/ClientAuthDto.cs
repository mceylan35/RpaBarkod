using Newtonsoft.Json;

namespace Shared.DTOs
{
    public class ClientAuthDto
    {
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }
    }

    public class AuthResponseDto
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("expiresIn")]
        public int ExpiresIn { get; set; }
    }
}