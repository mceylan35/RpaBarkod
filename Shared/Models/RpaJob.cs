using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Shared.Models
{
    public class RpaJob
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public RpaJob()
        {
            // Set expiration time to 2 hours from creation
            ExpiresAt = DateTime.UtcNow.AddHours(2);
        }

        [BsonElement("storeId")]
        [JsonProperty("storeId")]
        public string StoreId { get; set; }

        [BsonElement("product")]
        [JsonProperty("product")]
        public Product Product { get; set; }

        [BsonElement("status")]
        [JsonProperty("status")]
        public JobStatus Status { get; set; } = JobStatus.Pending;

        [BsonElement("clientId")]
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [BsonElement("retryCount")]
        [JsonProperty("retryCount")]
        public int RetryCount { get; set; } = 0;

        [BsonElement("maxRetries")]
        [JsonProperty("maxRetries")]
        public int MaxRetries { get; set; } = 3;

        [BsonElement("errorMessage")]
        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        [BsonElement("createdAt")]
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("startedAt")]
        [JsonProperty("startedAt")]
        public DateTime? StartedAt { get; set; }

        [BsonElement("completedAt")]
        [JsonProperty("completedAt")]
        public DateTime? CompletedAt { get; set; }

        [BsonElement("expiresAt")]
        [JsonProperty("expiresAt")]
        public DateTime? ExpiresAt { get; set; }

        [BsonElement("updatedAt")]
        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum JobStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Expired
    }
}