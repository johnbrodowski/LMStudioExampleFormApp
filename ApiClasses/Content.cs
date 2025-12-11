using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LMStudioExampleFormApp.ApiClasses
{
    public class Content
    {

        [JsonPropertyName("type")]
        public string Type { get; set; } // e.g., "text", "server_tool_use"

        [JsonPropertyName("id")]
        public string? Id { get; set; } // For server_tool_use, this is the tool_use_id

        [JsonPropertyName("name")]
        public string? Name { get; set; } // For server_tool_use, this is "web_search"

        [JsonPropertyName("input")]
        public ToolInput? Input { get; set; } // For server_tool_use, this will contain the 'query'

        [JsonPropertyName("text")]
        public string? Text { get; set; }    // For text blocks



        // Existing properties from your implied structure
        [JsonPropertyName("signature")]
        public string? Signature { get; set; }

        [JsonPropertyName("Index")]
        public int Index { get; set; }

        // Property for redacted_thinking data
        [JsonPropertyName("data")]
        public string? Data { get; set; }



    }

}
