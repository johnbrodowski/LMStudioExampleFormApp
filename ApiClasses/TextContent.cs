using LMStudioExampleFormApp.Interfaces;

using System.Text.Json.Serialization;

namespace LMStudioExampleFormApp.ApiClasses
{
    public class TextContent : IMessageContent
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; } = MessageType.Text;

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}