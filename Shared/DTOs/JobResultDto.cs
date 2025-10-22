using Newtonsoft.Json;
using Shared.Models;

namespace Shared.DTOs
{
    public class JobResultDto
    {
        [JsonProperty("jobId")]
        public string JobId { get; set; }

        [JsonProperty("status")]
        public JobStatus Status { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty("completedAt")]
        public DateTime? CompletedAt { get; set; }
    }
}