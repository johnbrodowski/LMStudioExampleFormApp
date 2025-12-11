using System.Text.Json.Serialization;           // For debugging output

//var systemMsg = Message.CreateSystemMessage("You are helpful");
//var userMsg = Message.CreateUserTextMessage("Hello!");
//var assistantMsg = Message.CreateAssistantMessage("Hi there!");


namespace LMStudioExampleFormApp.ApiClasses
{
    /// <summary>
    /// Detailed information about a specific model
    /// </summary>
    public class ModelInfo
	{
		[JsonPropertyName("id")]
		public string? Id { get; set; }

		[JsonPropertyName("object")]
		public string? Object { get; set; }

		[JsonPropertyName("type")]
		public string? Type { get; set; }

		[JsonPropertyName("publisher")]
		public string? Publisher { get; set; }

		[JsonPropertyName("arch")]
		public string? Arch { get; set; }

		[JsonPropertyName("compatibility_type")]
		public string? CompatibilityType { get; set; }

		[JsonPropertyName("quantization")]
		public string? Quantization { get; set; }

		[JsonPropertyName("state")]
		public string? State { get; set; }

		[JsonPropertyName("max_context_length")]
		public int MaxContextLength { get; set; }

		/// <summary>
		/// Helper property to check if model is currently loaded
		/// </summary>
		public bool IsLoaded => State?.ToLowerInvariant() == "loaded";

		/// <summary>
		/// Helper property to check if this is an embedding model
		/// </summary>
		public bool IsEmbeddingModel => Type?.ToLowerInvariant() == "embeddings";

		/// <summary>
		/// Helper property to check if this is a language model (LLM)
		/// </summary>
		public bool IsLanguageModel => Type?.ToLowerInvariant() == "llm";

		/// <summary>
		/// Helper property to check if this is a vision-language model (VLM)
		/// </summary>
		public bool IsVisionModel => Type?.ToLowerInvariant() == "vlm";

		/// <summary>
		/// Returns a human-readable Description of the model
		/// </summary>
		public override string ToString()
		{
			return $"{Id} ({Type}, {State}, {Quantization})";
		}
	}

	 

}
