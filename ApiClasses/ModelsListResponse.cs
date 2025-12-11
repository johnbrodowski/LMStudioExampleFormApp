using System.Text.Json.Serialization;           // For debugging output

//var systemMsg = Message.CreateSystemMessage("You are helpful");
//var userMsg = Message.CreateUserTextMessage("Hello!");
//var assistantMsg = Message.CreateAssistantMessage("Hi there!");


namespace LMStudioExampleFormApp.ApiClasses
{
    /// <summary>
    /// Response containing a list of all available models
    /// </summary>
    public class ModelsListResponse
	{
		[JsonPropertyName("object")]
		public string? Object { get; set; }

		[JsonPropertyName("data")]
		public ModelInfo[]? Data { get; set; }
	}

	 

}
