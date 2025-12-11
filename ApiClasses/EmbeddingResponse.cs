using System.Text.Json.Serialization;           // For debugging output

//var systemMsg = Message.CreateSystemMessage("You are helpful");
//var userMsg = Message.CreateUserTextMessage("Hello!");
//var assistantMsg = Message.CreateAssistantMessage("Hi there!");


namespace LMStudioExampleFormApp.ApiClasses
{
    // --- Embedding Response Models ---
    public class EmbeddingResponse
	{
		[JsonPropertyName("object")]
		public string? Object { get; set; }

		[JsonPropertyName("data")]
		public EmbeddingData[]? Data { get; set; }

		[JsonPropertyName("model")]
		public string? Model { get; set; }

		[JsonPropertyName("usage")]
		public EmbeddingUsage? Usage { get; set; }
	}

	 

}
