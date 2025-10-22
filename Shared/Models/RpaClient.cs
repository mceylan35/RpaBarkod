using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Shared.Models
{
    public class RpaClient
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("clientId")]
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [BsonElement("clientSecret")]
        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }

        [BsonElement("name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [BsonElement("isActive")]
        [JsonProperty("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("lastLoginAt")]
        [JsonProperty("lastLoginAt")]
        public DateTime? LastLoginAt { get; set; }

        [BsonElement("loginExpiresAt")]
        [JsonProperty("loginExpiresAt")]
        public DateTime? LoginExpiresAt { get; set; }

        [BsonElement("createdAt")]
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}