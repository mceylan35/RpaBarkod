using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Shared.Models
{
    public class Barcode
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("code")]
        [JsonProperty("code")]
        public string Code { get; set; }

        [BsonElement("isUsed")]
        [JsonProperty("isUsed")]
        public bool IsUsed { get; set; } = false;

        [BsonElement("usedByStoreId")]
        [JsonProperty("usedByStoreId")]
        public string UsedByStoreId { get; set; }

        [BsonElement("usedAt")]
        [JsonProperty("usedAt")]
        public DateTime? UsedAt { get; set; }

        [BsonElement("createdAt")]
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}