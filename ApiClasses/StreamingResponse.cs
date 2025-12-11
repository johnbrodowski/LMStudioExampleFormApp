using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMStudioExampleFormApp.ApiClasses
{
    // Model for streaming response format
    public class StreamingResponse
    {
        public string? id { get; set; }                // Unique identifier for the response
        public string? @object { get; set; }           // Type of object (usually "chat.completion.chunk")
        public int created { get; set; }               // Timestamp when the response was created
        public string? model { get; set; }             // Model ID that generated the response
        public StreamingChoice[]? choices { get; set; } // Array of content choices (usually just one)
    }
}
