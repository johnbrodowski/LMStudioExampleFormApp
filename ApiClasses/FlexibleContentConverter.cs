using LMStudioExampleFormApp.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LMStudioExampleFormApp.ApiClasses
{
    public class FlexibleContentConverter : JsonConverter<List<IMessageContent>>
    {
        public override List<IMessageContent>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Check if it's a string (API response format)
            if (reader.TokenType == JsonTokenType.String)
            {
                var textContent = reader.GetString();
                return new List<IMessageContent>
                {
                    new TextContent
                    {
                        Type = MessageType.Text,
                        Text = textContent
                    }
                };
            }
            // Otherwise, it's an array (our request format)
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                var list = new List<IMessageContent>();
                var contentConverter = new MessageContentConverter();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    var item = contentConverter.Read(ref reader, typeof(IMessageContent), options);
                    if (item != null)
                        list.Add(item);
                }

                return list;
            }

            return new List<IMessageContent>();
        }

        public override void Write(Utf8JsonWriter writer, List<IMessageContent> value, JsonSerializerOptions options)
        {
            // Always write as array
            writer.WriteStartArray();
            var contentConverter = new MessageContentConverter();

            foreach (var item in value)
            {
                contentConverter.Write(writer, item, options);
            }

            writer.WriteEndArray();
        }
    }

}
