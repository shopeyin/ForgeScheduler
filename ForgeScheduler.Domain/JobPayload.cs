using System.Text.Json.Serialization;

namespace ForgeScheduler.Domain
{
    public class JobPayload
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}
