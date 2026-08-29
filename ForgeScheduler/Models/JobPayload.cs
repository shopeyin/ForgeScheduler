using System.Text.Json.Serialization;

namespace ForgeScheduler.Models
{
    public class JobPayload
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}
