using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMStudioExampleFormApp.ApiClasses
{
    public class StreamingChoice
    {
        public int index { get; set; }                 // Index of this choice (usually 0)
        public DeltaMessage? delta { get; set; }       // The new content in this chunk
        public string? finish_reason { get; set; }     // Why the response finished (null, "stop", "length", "tool_calls", etc.)
    }
}
