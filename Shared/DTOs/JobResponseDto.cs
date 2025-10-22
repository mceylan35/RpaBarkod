using Newtonsoft.Json;
using Shared.Models;

namespace Shared.DTOs
{
    public class JobResponseDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("storeId")]
        public string StoreId { get; set; }

        [JsonProperty("product")]
        public Product Product { get; set; }

        [JsonProperty("status")]
        public JobStatus Status { get; set; }

        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("retryCount")]
        public int RetryCount { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("startedAt")]
        public DateTime? StartedAt { get; set; }

        [JsonProperty("completedAt")]
        public DateTime? CompletedAt { get; set; }
    }
}