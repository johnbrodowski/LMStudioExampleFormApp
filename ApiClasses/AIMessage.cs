namespace LMStudioExampleFormApp.ApiClasses
{
    public class AIMessage
    {
        public string? id { get; set; }                // Unique identifier for the response
        public string? @object { get; set; }           // Type of object (usually "chat.completion")
        public int created { get; set; }               // Timestamp when the response was created
        public string? model { get; set; }             // Model ID that generated the response
        public Choice[]? choices { get; set; }         // Array of content choices (usually just one)
        public Usage? usage { get; set; }              // Token usage statistics
    }
}