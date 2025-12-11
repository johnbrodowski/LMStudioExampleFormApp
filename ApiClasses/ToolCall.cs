using LMStudioExampleFormApp.ApiClasses;

using System.Text.Json.Serialization;           // For debugging output

//var systemMsg = Message.CreateSystemMessage("You are helpful");
//var userMsg = Message.CreateUserTextMessage("Hello!");
//var assistantMsg = Message.CreateAssistantMessage("Hi there!");


namespace LMStudioExampleFormApp
{
    // --- Tool Call Models ---
    public class ToolCall
	{
		[JsonPropertyName("id")]
		public string? Id { get; set; }

		[JsonPropertyName("type")]
		public string Type { get; set; } = "function";

		[JsonPropertyName("function")]
		public ToolCallFunction? Function { get; set; }
	}

	 

}
