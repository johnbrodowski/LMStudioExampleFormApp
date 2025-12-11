using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LMStudioExampleFormApp.ApiClasses
{
    public class DeltaMessage
    {
        public string? role { get; set; }              // Role (usually "assistant")
        public string? content { get; set; }           // The actual text content
        public string? reasoning { get; set; }         // Reasoning/thinking content (for models that support it)

        [JsonPropertyName("tool_calls")]
        public List<StreamingToolCall>? tool_calls { get; set; }  // Tool calls in streaming mode
    }
}
