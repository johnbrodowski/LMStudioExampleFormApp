using System;                       // Basic .NET functionality
using System.Net.Http;              // For making HTTP requests to the AI API
using System.Text;                  // For text encoding
using System.Text.Json;             // For JSON serialization/deserialization
using System.Threading;             // For cancellation support
using System.Threading.Tasks;       // For async programming
using System.Diagnostics;           // For debugging output

namespace LMStudioExampleFormApp
{
    // Main class that handles communication with LM Studio or other compatible AI services
    // Uses constructor with parameters: API endpoint URL, model name, and system prompt
    public class LMStudioExample(string endpoint, string model, string systemPrompt) : IDisposable
    {
        // HttpClient for making API requests - reused across all requests for efficiency
        private readonly HttpClient _httpClient = new HttpClient();
        // Flag to track whether resources have been disposed
        private bool _disposed = false;

        // --- Event declarations for notifying subscribers about request status ---

        // Event triggered when a piece of content is received in streaming mode
        public event EventHandler<string>? OnContentReceived;

        // Event triggered when the entire response is complete
        public event EventHandler<string>? OnComplete;

        // Event triggered when an error occurs
        public event EventHandler<Exception>? OnError;

        // Event triggered when the status of the request changes
        public event EventHandler<string>? OnStatusUpdate;

        // --- Method for streaming API requests ---
        // This method sends a message to the AI and gets the response in real-time chunks
        public async Task SendMessageAsync(string userMessage, CancellationToken cancellationToken = default)
        {
            // Validate input to avoid sending empty messages
            if (string.IsNullOrEmpty(userMessage))
                throw new ArgumentException("User message cannot be empty", nameof(userMessage));

            try
            {
                // Notify subscribers that we're starting a streaming request
                RaiseStatusUpdate("Sending streaming request...");

                // Build the request content in the format the API expects
                var requestContent = new
                {
                    model = model,                // Model name (e.g., "gemma-3-4b-it")
                    messages = new[] {            // Array of message objects
                        new { role = "system", content = systemPrompt },   // System instructions
                        new { role = "user", content = userMessage }       // User's input
                    },
                    temperature = 0.7,            // Controls randomness (0-1)
                    max_tokens = -1,              // Maximum length of response (-1 means no limit)
                    stream = true                 // Enable streaming mode
                };

                // Convert the request object to JSON
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestContent),  // Convert to JSON string
                    Encoding.UTF8,                             // Use UTF-8 encoding
                    "application/json");                       // Set content type to JSON

                // Create an HTTP request message
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Content = jsonContent;

                // Send the request with streaming option enabled
                // HttpCompletionOption.ResponseHeadersRead starts processing as soon as headers arrive
                var response = await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,   // Enable streaming
                    cancellationToken);                         // Allow cancellation

                // Check for HTTP error codes
                response.EnsureSuccessStatusCode();
                RaiseStatusUpdate("Processing streaming response...");

                // Process the streaming response
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                {
                    string fullResponse = "";  // Accumulate the full response

                    // Continue reading until the stream ends or cancellation is requested
                    while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                    {

                        // Read one line at a time
                        var line = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(line))
                            continue;  // Skip empty lines


                        // Server-Sent Events (SSE) format uses "data: " prefix
                        if (line.StartsWith("data: "))
                        {
                            // Extract the JSON data from the line
                            var jsonData = line.Substring(6).Trim();  // Remove "data: " prefix
                            if (jsonData == "[DONE]")
                                break;  // Special marker indicating end of stream

                            try
                            {
                                // Parse the JSON chunk into our StreamingResponse class
                                var chunk = JsonSerializer.Deserialize<StreamingResponse>(jsonData);
                                if (chunk?.choices != null && chunk.choices.Length > 0)
                                {
                                    var choice = chunk.choices[0];
                                    // Check if there's content in this chunk
                                    if (choice.delta != null && !string.IsNullOrEmpty(choice.delta.content))
                                    {
                                        var content = choice.delta.content;
                                        fullResponse += content;  // Add to accumulated response
                                        RaiseContentReceived(content);  // Notify subscribers
                                    }
                                }
                            }
                            catch (JsonException ex)
                            {
                                // Log JSON parsing errors but continue processing
                                Debug.WriteLine($"JSON parsing error: {ex.Message}");
                                Debug.WriteLine($"Problematic JSON: {jsonData}");
                            }
                        }
                    }
 
                    // If not cancelled, notify that the response is complete
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        RaiseComplete(fullResponse);
                    }
                    else
                    {
                        throw new OperationCanceledException("The operation was canceled.");
                    }

                }

            }
            catch (OperationCanceledException ex)
            {
                // Handle CanceledException
                RaiseError(ex);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the process
                RaiseError(ex);
            }
            finally
            {
                // If cancelled, notify that the response was cancelled
                if (cancellationToken.IsCancellationRequested)
                {
                    RaiseComplete($"Error: The operation was canceled.");
                }
            }
 
        }

        // --- Method for non-streaming API requests ---
        // This method sends a message and waits for the complete response before returning
        public async Task<string> SendMessageNonStreamingAsync(string userMessage, CancellationToken cancellationToken = default)
        {
            // Validate input
            if (string.IsNullOrEmpty(userMessage))
                throw new ArgumentException("User message cannot be empty", nameof(userMessage));

            try
            {
                // Notify subscribers about starting a non-streaming request
                RaiseStatusUpdate("Sending non-streaming request...");

                // Build the request content (similar to streaming, but with stream=false)
                var requestContent = new
                {
                    model = model,
                    messages = new[] {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userMessage }
                    },
                    temperature = 0.7,
                    max_tokens = -1,
                    stream = false  // Disable streaming
                };

                // Convert to JSON and prepare the content
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestContent),
                    Encoding.UTF8,
                    "application/json");

                // Send the request and wait for the full response
                var response = await _httpClient.PostAsync(endpoint, jsonContent, cancellationToken);
                response.EnsureSuccessStatusCode();

                RaiseStatusUpdate("Processing non-streaming response...");

                // Read the complete response JSON
                var jsonResponse = await response.Content.ReadAsStringAsync();
                // Deserialize into our AIMessage class
                var aiResponse = JsonSerializer.Deserialize<AIMessage>(jsonResponse);

                // Extract the message content
                if (aiResponse?.choices != null && aiResponse.choices.Length > 0)
                {
                    var responseContent = aiResponse?.choices[0]?.message?.content ?? "";
                    Debug.WriteLine($"Non-streaming response received: {responseContent.Length} characters");
                    RaiseComplete(responseContent  );  // Notify subscribers
                    return responseContent;  // Return the full response
                } //Error: The operation was canceled.
                else
                {
                    // Handle invalid response format
                    throw new Exception("Invalid response format");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                RaiseError(ex);
                throw;  // Re-throw to allow caller to handle the error
            }
        }

        // --- Helper methods for raising events ---

        // Notify subscribers when content is received (in streaming mode)
        private void RaiseContentReceived(string content)
        {
            Debug.WriteLine($"Content received: {content}");
            OnContentReceived?.Invoke(this, content);  // The '?' ensures we only call if there are subscribers
        }

        // Notify subscribers when the response is complete
        private void RaiseComplete(string fullResponse)
        {
            Debug.WriteLine($"Completed with full response of {fullResponse.Length} characters");
            OnComplete?.Invoke(this, fullResponse);
        }

        // Notify subscribers when an error occurs
        private void RaiseError(Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            OnError?.Invoke(this, ex);
        }

        // Notify subscribers of status updates
        private void RaiseStatusUpdate(string status)
        {
            Debug.WriteLine($"Status: {status}");
            OnStatusUpdate?.Invoke(this, status);
        }

        // --- IDisposable implementation to clean up resources ---

        // Public Dispose method called by clients
        public void Dispose()
        {
            Dispose(true);  // Dispose managed and unmanaged resources
            GC.SuppressFinalize(this);  // Tell GC not to call finalize method
        }

        // Protected method that actually performs the disposal
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)  // Only dispose once
            {
                if (disposing)  // If we're disposing managed resources
                {
                    _httpClient?.Dispose();  // Clean up the HttpClient
                }
                _disposed = true;  // Mark as disposed
            }
        }
    }

    // --- JSON response models for parsing API responses ---

    // Model for streaming response format
    public class StreamingResponse
    {
        public string? id { get; set; }                // Unique identifier for the response
        public string? @object { get; set; }           // Type of object (usually "chat.completion.chunk")
        public int created { get; set; }               // Timestamp when the response was created
        public string? model { get; set; }             // Model ID that generated the response
        public StreamingChoice[]? choices { get; set; } // Array of content choices (usually just one)
    }

    // Represents one chunk of a streaming response
    public class StreamingChoice
    {
        public int index { get; set; }                 // Index of this choice (usually 0)
        public DeltaMessage? delta { get; set; }       // The new content in this chunk
        public string? finish_reason { get; set; }     // Why the response finished (null, "stop", "length", etc.)
    }

    // Contains the actual content delta in a streaming response
    public class DeltaMessage
    {
        public string? role { get; set; }              // Role (usually "assistant")
        public string? content { get; set; }           // The actual text content
    }

    // --- JSON models for non-streaming responses ---

    // Model for complete, non-streaming API response
    public class AIMessage
    {
        public string? id { get; set; }                // Unique identifier for the response
        public string? @object { get; set; }           // Type of object (usually "chat.completion")
        public int created { get; set; }               // Timestamp when the response was created
        public string? model { get; set; }             // Model ID that generated the response
        public Choice[]? choices { get; set; }         // Array of content choices (usually just one)
        public Usage? usage { get; set; }              // Token usage statistics
    }

    // Represents one complete response option
    public class Choice
    {
        public int index { get; set; }                 // Index of this choice (usually 0)
        public Message? message { get; set; }          // The complete message
        public string? finish_reason { get; set; }     // Why the response finished ("stop", "length", etc.)
    }

    // Contains the complete message in a non-streaming response
    public class Message
    {
        public string? role { get; set; }              // Role (usually "assistant")
        public string? content { get; set; } = "";     // The complete text content
    }

    // Token usage information
    public class Usage
    {
        public int prompt_tokens { get; set; }         // Tokens used in the input prompt
        public int completion_tokens { get; set; }     // Tokens used in the generated response
        public int total_tokens { get; set; }          // Total tokens used
    }
}