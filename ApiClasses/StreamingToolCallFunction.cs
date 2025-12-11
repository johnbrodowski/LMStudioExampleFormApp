using System.Text.Json.Serialization;           // For debugging output

//var systemMsg = Message.CreateSystemMessage("You are helpful");
//var userMsg = Message.CreateUserTextMessage("Hello!");
//var assistantMsg = Message.CreateAssistantMessage("Hi there!");


namespace LMStudioExampleFormApp.ApiClasses
{
    // Represents the function part of a streaming tool call
    public class StreamingToolCallFunction
	{
		[JsonPropertyName("name")]
		public string? Name { get; set; }

		[JsonPropertyName("arguments")]
		public string? Arguments { get; set; }
	}

	 

}
