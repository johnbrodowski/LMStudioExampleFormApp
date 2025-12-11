using LMStudioExampleFormApp.Interfaces;

using System.Text.Json.Serialization;

namespace LMStudioExampleFormApp.ApiClasses
{
    public class MessageLMStudio
    {
        [JsonPropertyName("role")]
        public string role { get; set; }

        [JsonPropertyName("content")]
        public List<IMessageContent> content { get; set; }

        public MessageLMStudio(string role)
        {
            this.role = role;
            content = new List<IMessageContent>();
        }
    }
}