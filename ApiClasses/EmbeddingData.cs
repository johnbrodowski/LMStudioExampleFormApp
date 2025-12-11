using System.Text.Json.Serialization;           // For debugging output

//var systemMsg = Message.CreateSystemMessage("You are helpful");
//var userMsg = Message.CreateUserTextMessage("Hello!");
//var assistantMsg = Message.CreateAssistantMessage("Hi there!");


namespace LMStudioExampleFormApp.ApiClasses
{
    public class EmbeddingData
	{
		[JsonPropertyName("object")]
		public string? Object { get; set; }

		[JsonPropertyName("embedding")]
		public float[]? Embedding { get; set; }

		[JsonPropertyName("index")]
		public int Index { get; set; }
	}

	 

}
