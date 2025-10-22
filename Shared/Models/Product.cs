using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Shared.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("productCode")]
        [JsonProperty("productCode")]
        public string ProductCode { get; set; }

        [BsonElement("productName")]
        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [BsonElement("price")]
        [JsonProperty("price")]
        public decimal Price { get; set; }

        [BsonElement("barcode")]
        [JsonProperty("barcode")]
        public string Barcode { get; set; }

        [BsonElement("storeId")]
        [JsonProperty("storeId")]
        public string StoreId { get; set; }

        [BsonElement("createdAt")]
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}