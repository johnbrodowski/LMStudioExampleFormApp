using System;                       // Basic .NET functionality
using System.Net.Http;              // For making HTTP requests to the AI API
using System.Text;                  // For text encoding
using System.Text.Json;             // For JSON serialization/deserialization
using System.Threading;             // For cancellation support
using System.Threading.Tasks;       // For async programming
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace LMStudioExampleFormApp.MessageClass
{

    public interface IMessageContent
    {
        string type { get; }
        //string? text { get; set; } 
    }

    public static class MessageType
    {
        public const string Text = "text";
        public const string ImageUrl = "image_url";
        public const string ToolUse = "tool_use";
        public const string ToolResult = "tool_result"; 
        public const string Thinking = "thinking"; 
    }

    public class MessageClass
    {
        [JsonPropertyName("role")]
        public string role { get; set; }

        [JsonPropertyName("content")]
        public List<IMessageContent> content { get; set; }



        public MessageClass(string role)
        {
            this.role = role;
            content = new List<IMessageContent>();
        }


        public static MessageClass CreateUserMessage(string text)
        {
            var message = new MessageClass("user");
            message.content.Add(new MessageContent
            {
                type = MessageType.Text,
                text = text
            });

            return message;
        }



        public static MessageClass 
            CreateImageMessage(string text, string imageData)
        {
            var message = new MessageClass("user");

            var msgContentList = new List<IMessageContent>();

            msgContentList.Add(new ImageUrlData
            {
                type = "image_url",
                image_url = new ImageUrl { url = imageData }
            });

            msgContentList.Add(new MessageContent
            {
                type = "text",
                text = text
            });

            message.content = msgContentList;

            return message;
        }



    
         

         


        public static MessageClass CreateUserImageMessage(string text, string imageData)
        {
            var message = new MessageClass("User");

            var msgContentList = new List<IMessageContent>();


            msgContentList.Add(new ImageContent
            {
                source = new Source
                {
                    media_type = "Jpeg",
                    data = imageData
                }
            });

            msgContentList.Add(new MessageContent
            {
                text = text
            });
             

            message.content.Add(new ToolResultContentList
            {
                content = msgContentList
            });

             
            return message;
        }







        public static MessageClass CreateSystemTextMessage(string text )
        {
            var message = new MessageClass("User");
            message.content.Add(new MessageContent
            {
                text = text 
            });

            return message;
        }
 
        public static MessageClass CreateToolResultImageMessage(string toolUseId, string text, string imageData, bool isError)
        {
            var message = new MessageClass("User");

            var toolContent = new List<IMessageContent>();

            toolContent.Add(new MessageContent
            {
                text = text
            });


            toolContent.Add(new ImageContent
            {
                source = new Source
                {
                    media_type = "Jpeg",
                    data = imageData
                }
            });

            message.content.Add(new ToolResultContentList
            {
                tool_use_id = toolUseId,
                content = toolContent,
                is_error = isError
            });


            return message;
        }
         
        public static MessageClass CreateToolResultImageMessageTest(string toolUseId, string text, string imageData, bool isError)
        {
            var message = new MessageClass("User");

            var toolContent = new List<IMessageContent>();

            //toolContent.Add(new MessageContent
            //{
            //    text = text
            //});


            toolContent.Add(new ImageContent
            {
                source = new Source
                {
                    media_type = "Jpeg",
                    data = imageData
                }
            });

            message.content.Add(new ToolResultContentList
            {
                tool_use_id = toolUseId,
                content = toolContent,
                is_error = isError
            });


            return message;
        }
 
        public static MessageClass CreateToolResultImageMessage(string toolUseId, List<IMessageContent> toolContent, string imageData, bool isError)
        {
            var message = new MessageClass("User");
 

            message.content.Add(new ToolResultContentList
            {
                tool_use_id = toolUseId,
                content = toolContent,
                is_error = isError
            });


            return message;
        }
         
        public static ToolResultContentList CreateToolResultImageContent(string toolUseId, string text, string imageData, bool isError)
        {
 
            var tr = new ToolResultContentList
            {
                tool_use_id = toolUseId,
                content = new List<IMessageContent>()
                {
                    new MessageContent
                    {
                        text = text
                    },
                    new ImageContent
                    {
                        source = new Source
                        {
                            media_type = "Jpeg",
                            data = imageData
                        }
                    }
                },
                is_error = isError
            };


 
            return  tr;
        }
         
        public static MessageClass CreateToolResultMessage(string toolUseId)
        {
            var message = new MessageClass("User");

            message.content.Add(new ToolResultEmpty
            {
                tool_use_id = toolUseId
            });

            return message;
        }

        public static MessageClass CreateToolResultMessage(string toolUseId, string text)
        {
            var message = new MessageClass("User");

            message.content.Add(new ToolResultTextMessage
            {
                tool_use_id = toolUseId,
                content = text
            });

            return message;
        }

        public static MessageClass CreateToolResultMessage(string toolUseId, bool isError)
        {
            var message = new MessageClass("User");

            message.content.Add(new ToolResultEmptyWithError
            {
                tool_use_id = toolUseId,
                is_error = isError
            });

            return message;
        }

        public static MessageClass CreateToolResultMessage(string toolUseId, string text, bool isError)
        {
            var message = new MessageClass("User");

            message.content.Add(new ToolResultMessage
            {
                tool_use_id = toolUseId,
                content = text,
                is_error = isError
            });

            return message;
        }

        public static MessageClass CreateToolResultMessage(string toolUseId, List<IMessageContent> toolContent, bool isError = false)
        {
            var message = new MessageClass("User");

            message.content.Add(new ToolResultContentList
            {
                tool_use_id = toolUseId,
                content = toolContent,
                is_error = isError
            });

            if (isError)
            {
                message.content.Add(new MessageContent
                {
                    text = "Task completed with errors, prompt the user on how to proceed"
                });
            }
            else
            {
                //message.content.Add(new MessageContent
                //{
                //    text = "Task complete, prompt the user on how to proceed"
                //});
            }

            return message;
        }
          
        public static MessageClass CreateAssistantTextMessage(string text)
        {
            var message = new MessageClass("Assistant");
            message.content.Add(new MessageContent
            {
                text = text
            });

            return message;
        }
 
        public static MessageClass CreateToolUseMessage(string toolId, string text/*, ToolInput? input*/)
        {
            var message = new MessageClass("Assistant");
            message.content.Add(new ToolUseContent
            {
                text = text,
                id = toolId,
                //input = input
            });

            return message;
        }

        public static MessageClass CreateUserFileMessage(string text, string fileId, string fileType = "document")
        {
            var message = new MessageClass("user");

            message.content.Add(new FileContent(fileId, fileType));

            message.content.Add(new MessageContent
            {
                text = text
            });

            return message;
        }
         

    }


     


    public class ContentItem
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? type { get; set; } = null; // "text" or "image_url"

        [JsonPropertyName("text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? text { get; set; } = null; // Used when type is "text"

        [JsonPropertyName("image_url")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ImageUrl? image_url { get; set; } = null;// Used when type is "image_url"
    }

    public class ImageContent : IMessageContent
        {
            [JsonPropertyName("type")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string type { get; } = MessageType.ImageUrl;

            [JsonIgnore]
            public string? text { get; set; }

            [JsonPropertyName("source")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public Source source { get; set; } = new Source();
 
        }

   


 



 
    public class Source
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string type { get;} = "Base64";

        [JsonPropertyName("media_type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string media_type { get; set; } = "Jpeg";

        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string data { get; set; } = string.Empty;
    }
     
    public class ToolResultEmpty : IMessageContent
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string type { get; } = MessageType.ToolResult;

        [JsonIgnore]
        public string? text { get; set; }

        [JsonPropertyName("tool_use_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? tool_use_id { get; set; } = string.Empty; 
    }

    public class ToolResultEmptyWithError : IMessageContent
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string type { get; } = MessageType.ToolResult;

        [JsonIgnore]
        public string? text { get; set; }

        [JsonPropertyName("tool_use_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? tool_use_id { get; set; } = string.Empty;

        [JsonPropertyName("is_error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool is_error { get; set; } = false; 
    }

    public class ToolResultTextMessage : IMessageContent
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string type { get; } = MessageType.ToolResult;

        [JsonIgnore]
        public string? text { get; set; }

        [JsonPropertyName("tool_use_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? tool_use_id { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string content { get; set; } = string.Empty; 
    }

    public class ToolResultContentList : IMessageContent
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string type { get; } = MessageType.ToolResult;

        [JsonIgnore]
        public string? text { get; set; }

        [JsonPropertyName("tool_use_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? tool_use_id { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<IMessageContent>? content { get; set; }

        [JsonPropertyName("is_error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? is_error { get; set; }
         
    }
     
    public class ToolResultContent : IMessageContent
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string type { get; } = MessageType.ToolResult;

        [JsonIgnore]
        public string? text { get; set; }

        [JsonPropertyName("tool_use_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? tool_use_id { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IMessageContent? content { get; set; }

        [JsonPropertyName("is_error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? is_error { get; set; } 
    }
 
    public class ToolResultMessage : IMessageContent
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string type { get; } = MessageType.ToolResult;

        [JsonIgnore]
        public string? text { get; set; }

        [JsonPropertyName("tool_use_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? tool_use_id { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string content { get; set; } = string.Empty;

        [JsonPropertyName("is_error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool is_error { get; set; } = false; 
    }
 
    public class ToolUseContent : IMessageContent
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string type { get; set;} = MessageType.ToolUse;

        [JsonPropertyName("text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? text { get; set; }

        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? id { get; set; }
 
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? name { get; set; }

        //[JsonPropertyName("input")]
        //public ToolInput? input { get; set; }

 

        public ToolUseContent()
        {
            this.type = MessageType.ToolUse; 
        } // Default
    }

     
    //public class MessageLogEntry : IMessageContent
    //{
    //    [JsonPropertyName("type")]
    //    public string type { get; } = MessageType.ToolUse;

    //    [JsonPropertyName("text")]
    //    public string? text { get; set; }

    //    [JsonPropertyName("editor_id")]
    //    public string? editor_id { get; set; }

    //    [JsonPropertyName("role")]
    //    public string role { get; set; }

    //    [JsonPropertyName("content")]
    //    public List<IMessageContent> content { get; set; }

    //    [JsonPropertyName("content")]
    //    public CacheControl? CacheControl { get; set; }

    //    public MessageLogEntry()
    //    {
    //        content = new List<IMessageContent>();
    //    }
    //}
    public class SystemMessage : IMessageContent
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string type { get; } = MessageType.Text;

        [JsonPropertyName("text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? text { get; set; }

 
        public SystemMessage()
        {
 
        }

        public SystemMessage(string text )
        {
            type = "text";
            this.text = text; 
        }


        public static MessageClass CreateSystemTextMessage(string text)
        {
      

            var message = new MessageClass("System");
            message.content.Add(new MessageContent
            {
                text = text 
            });

            return message;
        }


    }





    public class MessageContent : IMessageContent
    {
        [JsonPropertyName("type")]
        public string type { get; set; } = MessageType.Text; // "text" or "image_url"

        [JsonPropertyName("text")]
        public string text { get; set; } = ""; // Used when type is "text"

        //[JsonPropertyName("image_url")]
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //public ImageUrl? image_url { get; set; } = null;// Used when type is "image_url"
    }

    // Image URL structure for image content



    // Image URL structure for image content
    public class ImageUrlData : IMessageContent
    {
        [JsonPropertyName("type")]
        public string type { get; set; } = MessageType.ImageUrl;

        [JsonPropertyName("image_url")]
        public ImageUrl image_url { get; set; } = new();// Used when type is "image_url"
    }

    public class ImageUrl
    {
        [JsonPropertyName("url")]
        public string url { get; set; } = ""; // Data URL with base64 encoded image
    }





    // Image URL structure for image content
    //public class ImageUrlData : IMessageContent
    //{
    //    [JsonPropertyName("type")]
    //    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    //    public string type { get; } = MessageType.image_url;

    //    [JsonPropertyName("url")]
    //    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    //    public string? url { get; set; } = null; // Data URL with base64 encoded image
    //}

     
    public class Thinking
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string type { get; set; } = "enabled";

        [JsonPropertyName("budget_tokens")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? BudgetTokens { get; set; }
    }
  
    // Models for thinking content in responses
    public class ThinkingContent : IMessageContent
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string type { get; } = MessageType.Thinking;

        [JsonPropertyName("thinking")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ThinkingText { get; set; }

        [JsonPropertyName("signature")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Signature { get; set; }

        [JsonIgnore]
        public string? text { get; set; } 
    }

    public class RedactedThinkingContent : IMessageContent
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string type { get; } = "RedactedThinking";

        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Data { get; set; }

        [JsonIgnore]
        public string? text { get; set; } 
    }

    public class FileContent : IMessageContent
    {
        // Make the "type" property read-only after instantiation.
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string type { get; }

        [JsonPropertyName("source")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public FileSource source { get; set; }

        [JsonIgnore]
        public string? text { get; set; } 

        public FileContent(string fileId, string fileType = "document")
        {
            this.type = fileType; // The type is set here and cannot be changed later
            this.source = new FileSource { file_id = fileId };
        }
    }

    public class FileSource
    {
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string type { get; } = "file";

        [JsonPropertyName("file_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string file_id { get; set; }
    }

}