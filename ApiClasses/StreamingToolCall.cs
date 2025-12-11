using System.Text.Json.Serialization;           // For debugging output

//var systemMsg = Message.CreateSystemMessage("You are helpful");
//var userMsg = Message.CreateUserTextMessage("Hello!");
//var assistantMsg = Message.CreateAssistantMessage("Hi there!");


namespace LMStudioExampleFormApp.ApiClasses
{
    // --- Image content implementation ---



    // --- Custom converter to handle both string and List<IMessageContentLMStudio> for content field ---

    // --- Image URL data structure ---


    // --- MessageLMStudio class with factory methods ---

    // --- JSON response models for parsing API responses ---





    // Represents one chunk of a streaming response


    // Contains the actual content delta in a streaming response


    // Represents a tool call delta in streaming mode
    public class StreamingToolCall
	{
		[JsonPropertyName("index")]
		public int Index { get; set; }

		[JsonPropertyName("id")]
		public string? Id { get; set; }

		[JsonPropertyName("type")]
		public string? Type { get; set; }

		[JsonPropertyName("function")]
		public StreamingToolCallFunction? Function { get; set; }
	}

	 

}
