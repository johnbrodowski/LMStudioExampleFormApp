using System.Text.Json.Serialization;           // For debugging output

//var systemMsg = Message.CreateSystemMessage("You are helpful");
//var userMsg = Message.CreateUserTextMessage("Hello!");
//var assistantMsg = Message.CreateAssistantMessage("Hi there!");


namespace LMStudioExampleFormApp.ApiClasses
{
    // --- Embedding Request Models ---
    public class EmbeddingRequest
	{
		[JsonPropertyName("model")]
		public string? Model { get; set; }

		[JsonPropertyName("input")]
		public string? Input { get; set; }
	}

	 

}
