using Newtonsoft.Json;

namespace Shared.DTOs
{
    public class JobRequestDto
    {
        [JsonProperty("storeId")]
        public string StoreId { get; set; }

        [JsonProperty("productCode")]
        public string ProductCode { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }
    }
}