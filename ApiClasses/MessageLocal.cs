using LMStudioExampleFormApp.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LMStudioExampleFormApp.ApiClasses
{
    public class MessageLocal
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        [JsonConverter(typeof(FlexibleContentConverter))]
        public List<IMessageContent> Content { get; set; }

        [JsonPropertyName("tool_calls")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ToolCall>? ToolCalls { get; set; }

        [JsonPropertyName("tool_call_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ToolCallId { get; set; }

        public MessageLocal(string role)
        {
            Role = role;
            Content = new List<IMessageContent>();
        }

        // Factory method to create a system message
        public static MessageLocal CreateSystemMessage(string text)
        {
            var message = new MessageLocal("system");
            message.Content.Add(new TextContent
            {
                Type = MessageType.Text,
                Text = text
            });
            return message;
        }

        // Factory method to create a user text message
        public static MessageLocal CreateUserTextMessage(string text)
        {
            var message = new MessageLocal("user");
            message.Content.Add(new TextContent
            {
                Type = MessageType.Text,
                Text = text
            });
            return message;
        }

        // Factory method to create an assistant message
        public static MessageLocal CreateAssistantMessage(string text)
        {
            var message = new MessageLocal("assistant");
            message.Content.Add(new TextContent
            {
                Type = MessageType.Text,
                Text = text
            });
            return message;
        }

        // Factory method to create an assistant message with tool calls
        public static MessageLocal CreateAssistantMessageWithToolCalls(List<ToolCall> toolCalls, string? content = null)
        {
            var message = new MessageLocal("assistant");

            if (!string.IsNullOrEmpty(content))
            {
                message.Content.Add(new TextContent
                {
                    Type = MessageType.Text,
                    Text = content
                });
            }

            message.ToolCalls = toolCalls;
            return message;
        }

        // Factory method to create a tool response message
        public static MessageLocal CreateToolMessage(string toolCallId, string content)
        {
            var message = new MessageLocal("tool");
            message.ToolCallId = toolCallId;
            message.Content.Add(new TextContent
            {
                Type = MessageType.Text,
                Text = content
            });
            return message;
        }

        // Helper method to get text content from the message
        public string GetTextContent()
        {
            if (Content == null) return "";

            var textItems = Content.OfType<TextContent>().Where(c => !string.IsNullOrEmpty(c.Text));
            return string.Join(" ", textItems.Select(c => c.Text));
        }
    }
}