using LMStudioExampleFormApp.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LMStudioExampleFormApp.ApiClasses
{
    // --- Custom JSON converter for polymorphic IMessageContentLMStudio ---
    public class MessageContentConverter : JsonConverter<IMessageContent>
    {
        public override IMessageContent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // For reading responses, we'll parse as JsonNode first
            var node = JsonNode.Parse(ref reader);
            if (node == null) return null;

            var typeValue = node["type"]?.GetValue<string>();

            return typeValue switch
            {
                MessageType.Text => node.Deserialize<TextContent>(options),
                MessageType.ImageUrl => node.Deserialize<ImageContent>(options),
                _ => null
            };
        }

        public override void Write(Utf8JsonWriter writer, IMessageContent value, JsonSerializerOptions options)
        {
            // Serialize the concrete type directly
            if (value is TextContent textContent)
            {
                System.Text.Json.JsonSerializer.Serialize(writer, textContent, options);
            }
            else if (value is ImageContent imageContent)
            {
                System.Text.Json.JsonSerializer.Serialize(writer, imageContent, options);
            }
        }
    }

}