using LMStudioExampleFormApp.Interfaces;

using System.Text.Json.Serialization;

namespace LMStudioExampleFormApp.ApiClasses
{
    public class ToolUseContent : IMessageContent
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = MessageType.ToolUse;

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("input")]
        public ToolInput? Input { get; set; }

        public ToolUseContent()
        {
            this.Type = MessageType.ToolUse;
        }
    }
}