
using System.Text.Json.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Drawing;


namespace LMStudioExampleFormApp.ApiClasses
{
    /// <summary>
    /// Represents a dynamic Editor object that manages Editor instances and their states
    /// </summary>
    public class ToolResultObject
    {
 
        [JsonIgnore]
        public ToolResultContent dynamic_tool_result { get; set; } = new();
 
    }

    public class ToolResultContent
    {
        [JsonPropertyName("success")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? success { get; set; }

        [JsonPropertyName("is_error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? is_error { get; set; }

        [JsonPropertyName("output_content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? output_content { get; set; } = new();

    }
}