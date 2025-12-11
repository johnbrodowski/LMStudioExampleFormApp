//var systemMsg = Message.CreateSystemMessage("You are helpful");
//var userMsg = Message.CreateUserTextMessage("Hello!");
//var assistantMsg = Message.CreateAssistantMessage("Hi there!");

namespace LMStudioExampleFormApp.ApiClasses
{
    // Represents one complete response option
    public class Choice
	{
		public int index { get; set; }                 // Index of this choice (usually 0)
		public MessageLocal? message { get; set; }          // The complete message
		public string? finish_reason { get; set; }     // Why the response finished ("stop", "length", etc.)
	}

	 

}
