using System.Text.Json.Serialization;

namespace LMStudioExampleFormApp.ApiClasses
{
    public class LMStudioResponse
    {
        [JsonPropertyName("content")]
        public List<Content> content { get; set; } = new List<Content>();
    }
}