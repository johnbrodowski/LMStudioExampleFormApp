//var systemMsg = Message.CreateSystemMessage("You are helpful");
//var userMsg = Message.CreateUserTextMessage("Hello!");
//var assistantMsg = Message.CreateAssistantMessage("Hi there!");

namespace LMStudioExampleFormApp.ApiClasses
{
    // Token usage information
    public class Usage
	{
		public int prompt_tokens { get; set; }         // Tokens used in the input prompt
		public int completion_tokens { get; set; }     // Tokens used in the generated response
		public int total_tokens { get; set; }          // Total tokens used
	}

	 

}
