using LMStudioExampleFormApp.ApiClasses;

using System;                       // Basic .NET functionality
using System.Collections.Generic;   // For List<T>
using System.Diagnostics;
using System.IO;                    // For file operations
using System.Linq;
using System.Net.Http;              // For making HTTP requests to the AI API
using System.Text;                  // For text encoding
using System.Text.Json;             // For JSON serialization/deserialization
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;           // For debugging output
using System.Threading;             // For cancellation support
using System.Threading.Tasks;

using JsonException = System.Text.Json.JsonException;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;       // For async programming

//var systemMsg = Message.CreateSystemMessage("You are helpful");
//var userMsg = Message.CreateUserTextMessage("Hello!");
//var assistantMsg = Message.CreateAssistantMessage("Hi there!");


namespace LMStudioExampleFormApp
{
	// Main class that handles communication with LM Studio or other compatible AI services
	// Uses constructor with parameters: API endpoint URL, model name, and system prompt
	public class LMStudioExample : IDisposable
	{
		// HttpClient for making API requests - reused across all requests for efficiency
		private readonly HttpClient _httpClient = new HttpClient
		{
			Timeout = TimeSpan.FromSeconds(100) // Set default timeout on creation
		};
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

		// Event triggered when tool calls are received from the model
		public event EventHandler<List<ToolCall>>? OnToolCallsReceived;

		private List<MessageLocal> messages = new();

		// Property to access the last tool calls received (if any)
		public List<ToolLMStudio>? tools { get; set; }
		public object model { get; private set; }
		public string? endpoint { get; private set; }

		private CancellationTokenSource _cancellationTokenSource;
		private string systemPrompt;
 
		public LMStudioExample(string endpoint, string model, string systemPrompt, List<ToolLMStudio>? _tools = null)
		{
			// gpt populate this
			this.endpoint = endpoint;
			this.model = model;
			this.systemPrompt = systemPrompt;
			this.tools = _tools;


		}
 

		public void RequestStop()
		{

			try
			{
				_cancellationTokenSource?.Cancel();
				//TriggerStreamingEvent(StreamingEventType.Cancelled, "Request was cancelled by user");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error during cancellation: {ex.Message}");
			}
		}


		public void initialize(long timeoutInSeconds = 100)
		{
			// Note: Timeout is now set when HttpClient is created (line 29-32)
			// Cannot modify HttpClient.Timeout after first request is made

			messages = new List<MessageLocal>()
			{
				MessageLocal.CreateSystemMessage(systemPrompt)
			};
		}

		[Obsolete("Timeout must be set when HttpClient is created. Use constructor parameter instead.")]
		public void SetTimeout(long timeSpanInSeconds)
		{
			// Cannot modify HttpClient.Timeout after it has been used for requests
			Debug.WriteLine("Warning: SetTimeout called but timeout is set at HttpClient creation. This call has no effect.");
		}


		// Method to send a text-only message (maintains backward compatibility)
		public async Task<string> SendMessageAsync(string userMessage, List<ToolLMStudio>? tools = null)
		{

			_cancellationTokenSource?.Dispose();

			// Create a new token source for this request
			_cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = _cancellationTokenSource.Token;
			 
			return await SendMessageAsync(userMessage, null, cancellationToken, tools);
		}

 
		 

		// Method to send a message with optional images and tools
		public async Task<string> SendMessageAsync(string? userMessage, string[]? imagePaths, CancellationToken cancellationToken = default, List<ToolLMStudio>? tools = null)
		{
			// Only require a message if this is the first message in the conversation
			// Allow empty messages when continuing a conversation (e.g., after tool results)
			if (string.IsNullOrEmpty(userMessage) && messages.Count == 0)
				throw new ArgumentException("User message cannot be empty for the first message", nameof(userMessage));

			// Only create and add user message if one was provided
			MessageLocal? msg = null;
			if (!string.IsNullOrEmpty(userMessage))
			{
				// Create user message with text
				msg = MessageLocal.CreateUserTextMessage(userMessage);
			}

			// Add images if provided (only if we have a message to add them to)
			if (msg != null && imagePaths != null && imagePaths.Length > 0)
			{
				foreach (var imagePath in imagePaths)
				{
					if (!File.Exists(imagePath))
					{
						throw new FileNotFoundException($"Image file not found: {imagePath}");
					}

					try
					{
						// Read image file and convert to base64
						var imageBytes = await File.ReadAllBytesAsync(imagePath);
						var base64String = Convert.ToBase64String(imageBytes);

						// Determine MIME type based on file extension
						var mimeType = GetMimeType(imagePath);
						var dataUrl = $"data:{mimeType};base64,{base64String}";

						msg.Content.Add(new ImageContent
						{
							Type = MessageType.ImageUrl,
							ImageUrl = new ImageUrlData { Url = dataUrl }
						});
					}
					catch (Exception ex)
					{
						throw new Exception($"Failed to process image {imagePath}: {ex.Message}", ex);
					}
				}
			}

			// Add the user message to the conversation history (if we created one)
			if(msg != null) this.messages.Add(msg);

			// Clear previous tool calls
			//    LastToolCalls = null;

			try
			{
				// Notify subscribers that we're starting a streaming request
				RaiseStatusUpdate("Sending streaming request...");

				// Build the request content in the format the API expects
				object requestContent;
				if (tools != null && tools.Count > 0)
				{
					requestContent = new
					{
						model = model,                // Model name (e.g., "gemma-3-4b-it")
						messages = this.messages,
						temperature = 0.7,            // Controls randomness (0-1)
						max_tokens = -1,              // Maximum length of response (-1 means no limit)
						stream = true,                // Enable streaming mode
						tools = tools                 // Tool definitions
					};
				}
				else
				{
					requestContent = new
					{
						model = model,                // Model name (e.g., "gemma-3-4b-it")
						messages = this.messages,
						temperature = 0.7,            // Controls randomness (0-1)
						max_tokens = -1,              // Maximum length of response (-1 means no limit)
						stream = true                 // Enable streaming mode
					};
				}

				// Convert the request object to JSON
				var jsonOptions = new JsonSerializerOptions
				{
					DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
					Converters = { new MessageContentConverter() }
				};

				var jsonContent = new StringContent(
					System.Text.Json.JsonSerializer.Serialize(requestContent, jsonOptions),  // Convert to JSON string
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
					var toolCallsAccumulator = new Dictionary<int, ToolCall>();  // Accumulate tool calls by index

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
							Debug.WriteLine(line);
 
							// Extract the JSON data from the line
							var jsonData = line.Substring(6).Trim();  // Remove "data: " prefix
							if (jsonData == "[DONE]")
								break;  // Special marker indicating end of stream

							try
							{
								// Parse the JSON chunk into our StreamingResponse class
								var chunk = System.Text.Json.JsonSerializer.Deserialize<StreamingResponse>(jsonData);
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

									// Check if there are tool calls in this chunk
									if (choice.delta?.tool_calls != null)
									{
										foreach (var toolCallDelta in choice.delta.tool_calls)
										{
											if (!toolCallsAccumulator.ContainsKey(toolCallDelta.Index))
											{
												toolCallsAccumulator[toolCallDelta.Index] = new ToolCall
												{
													Id = toolCallDelta.Id ?? "",
													Type = toolCallDelta.Type ?? "function",
													Function = new ToolCallFunction
													{
														Name = toolCallDelta.Function?.Name ?? "",
														Arguments = toolCallDelta.Function?.Arguments ?? ""
													}
												};
											}
											else
											{
												// Accumulate the streaming pieces
												var existingToolCall = toolCallsAccumulator[toolCallDelta.Index];
												if (!string.IsNullOrEmpty(toolCallDelta.Id))
													existingToolCall.Id += toolCallDelta.Id;
												if (!string.IsNullOrEmpty(toolCallDelta.Type))
													existingToolCall.Type = toolCallDelta.Type;
												if (toolCallDelta.Function != null)
												{
													if (existingToolCall.Function == null)
														existingToolCall.Function = new ToolCallFunction();

													if (!string.IsNullOrEmpty(toolCallDelta.Function.Name))
														existingToolCall.Function.Name += toolCallDelta.Function.Name;
													if (!string.IsNullOrEmpty(toolCallDelta.Function.Arguments))
														existingToolCall.Function.Arguments += toolCallDelta.Function.Arguments;
												}
											}
										}
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

					// Create assistant message with tool calls if any were accumulated
					MessageLocal msgAssistant;
					if (toolCallsAccumulator.Count > 0)
					{
						var toolCallsList = toolCallsAccumulator.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();

						// Fix any incorrectly nested tool arguments
						foreach (var toolCall in toolCallsList)
						{
							FixToolCallArguments(toolCall);
						}

						msgAssistant = MessageLocal.CreateAssistantMessageWithToolCalls(toolCallsList,
							string.IsNullOrEmpty(fullResponse) ? null : fullResponse);

						// Store the tool calls and notify subscribers
						//  LastToolCalls = toolCallsList;
						RaiseToolCallsReceived(toolCallsList);
					}
					else
					{
						msgAssistant = MessageLocal.CreateAssistantMessage(fullResponse);
					}
					this.messages.Add(msgAssistant);

					// If not cancelled, notify that the response is complete
					if (!cancellationToken.IsCancellationRequested)
					{
						RaiseComplete(fullResponse);
						return fullResponse;
					}
					else
					{
						throw new OperationCanceledException("The operation was canceled.");
					}
				}
			}
			catch (OperationCanceledException ex)
			{
				// If cancelled, notify that the response was cancelled
				if (cancellationToken.IsCancellationRequested)
				{
					RaiseComplete($"\nComplete: The operation was canceled.");
				}
			}
			catch (Exception ex)
			{
				// Handle any exceptions that occur during the process
				RaiseError(ex);
			}
			finally
			{

			}

			return "";  // Return empty string if no response was received
		}
		 
  
		public void ClearMessages(string? _systemPrompt = null)
		{
			if (!string.IsNullOrEmpty(_systemPrompt))
			{
				systemPrompt = _systemPrompt;
			}
			messages.Clear();
			messages.Add(MessageLocal.CreateSystemMessage(systemPrompt));
		}

		/// <summary>
		/// Adds a tool result to the message history. Call this after executing a tool that was requested by the model.
		/// </summary>
		/// <param name="toolCallId">The ID of the tool call (from ToolCall.Id)</param>
		/// <param name="result">The result of executing the tool (as a string, typically JSON)</param>
		public void AddToolResult(string toolCallId, string result)
		{
			var toolMessage = MessageLocal.CreateToolMessage(toolCallId, result);
			messages.Add(toolMessage);
		}




		public async Task<string> SendMessageWithImagesNonStreamingAsync(string userMessage, string[]? imagePaths = null, CancellationToken cancellationToken = default, List<ToolLMStudio>? tools = null)
		{
			// Validate input
			if (string.IsNullOrEmpty(userMessage))
				throw new ArgumentException("User message cannot be empty", nameof(userMessage));

			try
			{
				// Notify subscribers about starting a non-streaming request
				RaiseStatusUpdate("Sending non-streaming request...");

				// Create user message with text
				var msg = MessageLocal.CreateUserTextMessage(userMessage);

				// Add images if provided
				if (imagePaths != null && imagePaths.Length > 0)
				{
					foreach (var imagePath in imagePaths)
					{
						if (!File.Exists(imagePath))
						{
							throw new FileNotFoundException($"Image file not found: {imagePath}");
						}

						try
						{
							// Read image file and convert to base64
							var imageBytes = await File.ReadAllBytesAsync(imagePath);
							var base64String = Convert.ToBase64String(imageBytes);

							// Determine MIME type based on file extension
							var mimeType = GetMimeType(imagePath);
							var dataUrl = $"data:{mimeType};base64,{base64String}";

							msg.Content.Add(new ImageContent
							{
								Type = MessageType.ImageUrl,
								ImageUrl = new ImageUrlData { Url = dataUrl }
							});
						}
						catch (Exception ex)
						{
							throw new Exception($"Failed to process image {imagePath}: {ex.Message}", ex);
						}
					}
				}

				this.messages.Add(msg);

				// Clear previous tool calls
				//   LastToolCalls = null;

				// Build the request content (similar to streaming, but with stream=false)
				object requestContent;
				if (tools != null && tools.Count > 0)
				{
					requestContent = new
					{
						model = model,
						messages = this.messages,
						temperature = 0.7,
						max_tokens = -1,
						stream = false,  // Disable streaming
						tools = tools    // Tool definitions
					};
				}
				else
				{
					requestContent = new
					{
						model = model,
						messages = this.messages,
						temperature = 0.7,
						max_tokens = -1,
						stream = false  // Disable streaming
					};
				}

				// Convert to JSON and prepare the content
				var jsonOptions = new JsonSerializerOptions
				{
					DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
					Converters = { new MessageContentConverter() }
				};

				var jsonContent = new StringContent(
					System.Text.Json.JsonSerializer.Serialize(requestContent, jsonOptions),
					Encoding.UTF8,
					"application/json");

				Debug.WriteLine($"\n{requestContent.ToString()}\n");
				 
				// Send the request and wait for the full response
				var response = await _httpClient.PostAsync(endpoint, jsonContent, cancellationToken);
				 
				response.EnsureSuccessStatusCode();

				RaiseStatusUpdate("Processing non-streaming response...");

				// Read the complete response JSON
				var jsonResponse = await response.Content.ReadAsStringAsync();

				// Deserialize with custom options
				jsonOptions = new JsonSerializerOptions
				{
					DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
					Converters = { new MessageContentConverter() }
				};

				Debug.WriteLine($"\n{jsonResponse}\n");
 
				var aiResponse = System.Text.Json.JsonSerializer.Deserialize<AIMessage>(jsonResponse, jsonOptions);

				// Extract the message content
				if (aiResponse?.choices != null && aiResponse.choices.Length > 0)
				{
					var responseMessage = aiResponse.choices[0].message;
					var responseContent = responseMessage?.GetTextContent() ?? "";

					// Check if there are tool calls
					if (responseMessage?.ToolCalls != null && responseMessage.ToolCalls.Count > 0)
					{
						Debug.WriteLine($"Non-streaming response with tool calls: {responseMessage.ToolCalls.Count} tool(s)");

						// Fix any incorrectly nested tool arguments
						foreach (var toolCall in responseMessage.ToolCalls)
						{
							FixToolCallArguments(toolCall);
						}

						var msgAssistant = MessageLocal.CreateAssistantMessageWithToolCalls(
							responseMessage.ToolCalls,
							string.IsNullOrEmpty(responseContent) ? null : responseContent);
						this.messages.Add(msgAssistant);

						// Store the tool calls and notify subscribers
						//  LastToolCalls = responseMessage.ToolCalls;
						RaiseToolCallsReceived(responseMessage.ToolCalls);
					}
					else
					{
						Debug.WriteLine($"Non-streaming response received: {responseContent.Length} characters");
						var msgAssistant = MessageLocal.CreateAssistantMessage(responseContent);
						this.messages.Add(msgAssistant);
					}

					RaiseComplete(responseContent);  // Notify subscribers
					return responseContent;  // Return the full response
				}
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

		 
		/// <summary>
		/// Requests text embeddings from the LM Studio API
		/// </summary>
		/// <param name="text">The text to generate embeddings for</param>
		/// <param name="embeddingModel">The embedding model to use (e.g., "text-embedding-nomic-embed-text-v1.5")</param>
		/// <param name="cancellationToken">Cancellation token for async operation</param>
		/// <returns>Array of embedding vectors (float arrays)</returns>
		public async Task<float[]?> GetEmbeddingAsync(
			string text,
			string embeddingModel = "text-embedding-nomic-embed-text-v1.5",
			CancellationToken cancellationToken = default)
		{
			// Validate input
			if (string.IsNullOrEmpty(text))
				throw new ArgumentException("Text cannot be empty", nameof(text));

			if (string.IsNullOrEmpty(embeddingModel))
				throw new ArgumentException("Embedding model cannot be empty", nameof(embeddingModel));

			try
			{
				RaiseStatusUpdate("Requesting embeddings...");

				// Build the embedding request
				var requestContent = new EmbeddingRequest
				{
					Model = embeddingModel,
					Input = text
				};

				// Serialize to JSON
				var jsonContent = new StringContent(
					System.Text.Json.JsonSerializer.Serialize(requestContent),
					Encoding.UTF8,
					"application/json");

				// Construct the embedding endpoint URL
				// Replace the chat completions endpoint with the embeddings endpoint
				var embeddingEndpoint = endpoint.Replace("/v1/chat/completions", "/api/v0/embeddings");

				Debug.WriteLine($"Sending embedding request to: {embeddingEndpoint}");

				// Send the request
				var response = await _httpClient.PostAsync(
					embeddingEndpoint,
					jsonContent,
					cancellationToken);

				response.EnsureSuccessStatusCode();

				RaiseStatusUpdate("Processing embedding response...");

				// Read and parse the response
				var jsonResponse = await response.Content.ReadAsStringAsync();
				var embeddingResponse = System.Text.Json.JsonSerializer.Deserialize<EmbeddingResponse>(jsonResponse);

				// Extract the embedding vector
				if (embeddingResponse?.Data != null && embeddingResponse.Data.Length > 0)
				{
					var embedding = embeddingResponse.Data[0].Embedding;
					Debug.WriteLine($"Embedding received: {embedding?.Length ?? 0} dimensions");
					RaiseStatusUpdate("Embedding completed");
					return embedding;
				}
				else
				{
					throw new Exception("Invalid embedding response format");
				}
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				throw;
			}
		}

		/// <summary>
		/// Requests embeddings for multiple texts in a single batch request
		/// </summary>
		/// <param name="texts">Array of texts to generate embeddings for</param>
		/// <param name="embeddingModel">The embedding model to use</param>
		/// <param name="cancellationToken">Cancellation token for async operation</param>
		/// <returns>Array of embedding vectors, one for each input text</returns>
		public async Task<float[][]?> GetEmbeddingsBatchAsync(
			string[] texts,
			string embeddingModel = "text-embedding-nomic-embed-text-v1.5",
			CancellationToken cancellationToken = default)
		{
			if (texts == null || texts.Length == 0)
				throw new ArgumentException("Texts array cannot be null or empty", nameof(texts));

			try
			{
				RaiseStatusUpdate($"Requesting embeddings for {texts.Length} texts...");

				var embeddings = new List<float[]>();

				// Process each text individually
				// Note: Some APIs support batch input as an array, but we'll process sequentially for compatibility
				foreach (var text in texts)
				{
					var embedding = await GetEmbeddingAsync(text, embeddingModel, cancellationToken);
					if (embedding != null)
					{
						embeddings.Add(embedding);
					}
				}

				Debug.WriteLine($"Batch embeddings completed: {embeddings.Count} embeddings generated");
				RaiseStatusUpdate("Batch embedding completed");

				return embeddings.ToArray();
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				throw;
			}
		}












		/// <summary>
		/// Gets a list of all available models (both loaded and downloaded)
		/// </summary>
		/// <param name="cancellationToken">Cancellation token for async operation</param>
		/// <returns>Array of ModelInfo objects</returns>
		public async Task<ModelInfo[]?> GetAllModelsAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				RaiseStatusUpdate("Fetching models list...");

				// Construct the models endpoint URL
				var modelsEndpoint = endpoint.Replace("/v1/chat/completions", "/api/v0/models");

				Debug.WriteLine($"Fetching models from: {modelsEndpoint}");

				// Send GET request to the models endpoint
				var response = await _httpClient.GetAsync(modelsEndpoint, cancellationToken);
				response.EnsureSuccessStatusCode();

				RaiseStatusUpdate("Processing models list...");

				// Read and parse the response
				var jsonResponse = await response.Content.ReadAsStringAsync();
				var modelsResponse = System.Text.Json.JsonSerializer.Deserialize<ModelsListResponse>(jsonResponse);

				if (modelsResponse?.Data != null)
				{
					Debug.WriteLine($"Found {modelsResponse.Data.Length} models");
					RaiseStatusUpdate($"Found {modelsResponse.Data.Length} models");
					return modelsResponse.Data;
				}
				else
				{
					throw new Exception("Invalid models list response format");
				}
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				throw;
			}
		}

		/// <summary>
		/// Gets detailed information about a specific model by its ID
		/// </summary>
		/// <param name="modelId">The model ID (e.g., "qwen2-vl-7b-instruct")</param>
		/// <param name="cancellationToken">Cancellation token for async operation</param>
		/// <returns>ModelInfo object with detailed information</returns>
		public async Task<ModelInfo?> GetModelInfoAsync(string modelId, CancellationToken cancellationToken = default)
		{
			// Validate input
			if (string.IsNullOrEmpty(modelId))
				throw new ArgumentException("Model ID cannot be empty", nameof(modelId));

			try
			{
				RaiseStatusUpdate($"Fetching info for model: {modelId}...");

				// Construct the specific model endpoint URL
				var modelEndpoint = endpoint.Replace("/v1/chat/completions", $"/api/v0/models/{modelId}");

				Debug.WriteLine($"Fetching model info from: {modelEndpoint}");

				// Send GET request to the specific model endpoint
				var response = await _httpClient.GetAsync(modelEndpoint, cancellationToken);
				response.EnsureSuccessStatusCode();

				RaiseStatusUpdate("Processing model info...");

				// Read and parse the response
				var jsonResponse = await response.Content.ReadAsStringAsync();
				var modelInfo = System.Text.Json.JsonSerializer.Deserialize<ModelInfo>(jsonResponse);

				if (modelInfo != null)
				{
					Debug.WriteLine($"Model info retrieved: {modelInfo}");
					RaiseStatusUpdate($"Model info retrieved: {modelInfo.Id}");
					return modelInfo;
				}
				else
				{
					throw new Exception("Invalid model info response format");
				}
			}
			catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				// Handle case where model doesn't exist
				var notFoundEx = new Exception($"Model '{modelId}' not found", ex);
				RaiseError(notFoundEx);
				throw notFoundEx;
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				throw;
			}
		}

		/// <summary>
		/// Gets all loaded models (models currently in memory and ready to use)
		/// </summary>
		/// <param name="cancellationToken">Cancellation token for async operation</param>
		/// <returns>Array of loaded ModelInfo objects</returns>
		public async Task<ModelInfo[]?> GetLoadedModelsAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				var allModels = await GetAllModelsAsync(cancellationToken);

				if (allModels == null)
					return null;

				// Filter for only loaded models
				var loadedModels = allModels.Where(m => m.IsLoaded).ToArray();

				Debug.WriteLine($"Found {loadedModels.Length} loaded models out of {allModels.Length} total");
				RaiseStatusUpdate($"Found {loadedModels.Length} loaded models");

				return loadedModels;
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				throw;
			}
		}

		/// <summary>
		/// Gets all embedding models (both loaded and not loaded)
		/// </summary>
		/// <param name="cancellationToken">Cancellation token for async operation</param>
		/// <returns>Array of embedding ModelInfo objects</returns>
		public async Task<ModelInfo[]?> GetEmbeddingModelsAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				var allModels = await GetAllModelsAsync(cancellationToken);

				if (allModels == null)
					return null;

				// Filter for only embedding models
				var embeddingModels = allModels.Where(m => m.IsEmbeddingModel).ToArray();

				Debug.WriteLine($"Found {embeddingModels.Length} embedding models");
				RaiseStatusUpdate($"Found {embeddingModels.Length} embedding models");

				return embeddingModels;
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				throw;
			}
		}

		/// <summary>
		/// Gets all language models (LLMs)
		/// </summary>
		/// <param name="cancellationToken">Cancellation token for async operation</param>
		/// <returns>Array of LLM ModelInfo objects</returns>
		public async Task<ModelInfo[]?> GetLanguageModelsAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				var allModels = await GetAllModelsAsync(cancellationToken);

				if (allModels == null)
					return null;

				// Filter for only language models
				var llmModels = allModels.Where(m => m.IsLanguageModel).ToArray();

				Debug.WriteLine($"Found {llmModels.Length} language models");
				RaiseStatusUpdate($"Found {llmModels.Length} language models");

				return llmModels;
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				throw;
			}
		}

		/// <summary>
		/// Gets all vision-language models (VLMs)
		/// </summary>
		/// <param name="cancellationToken">Cancellation token for async operation</param>
		/// <returns>Array of VLM ModelInfo objects</returns>
		public async Task<ModelInfo[]?> GetVisionModelsAsync(CancellationToken cancellationToken = default)
		{
			try
			{
				var allModels = await GetAllModelsAsync(cancellationToken);

				if (allModels == null)
					return null;

				// Filter for only vision models
				var visionModels = allModels.Where(m => m.IsVisionModel).ToArray();

				Debug.WriteLine($"Found {visionModels.Length} vision-language models");
				RaiseStatusUpdate($"Found {visionModels.Length} vision-language models");

				return visionModels;
			}
			catch (Exception ex)
			{
				RaiseError(ex);
				throw;
			}
		}



		/// <summary>
		/// Calculates cosine similarity between two embedding vectors
		/// Useful for comparing semantic similarity between texts
		/// </summary>
		/// <param name="embedding1">First embedding vector</param>
		/// <param name="embedding2">Second embedding vector</param>
		/// <returns>Similarity score between -1 and 1 (1 = identical, 0 = orthogonal, -1 = opposite)</returns>
		public static float CalculateCosineSimilarity(float[] embedding1, float[] embedding2)
		{
			if (embedding1 == null || embedding2 == null)
				throw new ArgumentNullException("Embeddings cannot be null");

			if (embedding1.Length != embedding2.Length)
				throw new ArgumentException("Embeddings must have the same dimensions");

			float dotProduct = 0f;
			float magnitude1 = 0f;
			float magnitude2 = 0f;

			for (int i = 0; i < embedding1.Length; i++)
			{
				dotProduct += embedding1[i] * embedding2[i];
				magnitude1 += embedding1[i] * embedding1[i];
				magnitude2 += embedding2[i] * embedding2[i];
			}

			magnitude1 = (float)Math.Sqrt(magnitude1);
			magnitude2 = (float)Math.Sqrt(magnitude2);

			if (magnitude1 == 0f || magnitude2 == 0f)
				return 0f;

			return dotProduct / (magnitude1 * magnitude2);
		}


		// Helper method to determine MIME type from file extension
		private string GetMimeType(string filePath)
		{
			var extension = Path.GetExtension(filePath).ToLowerInvariant();
			return extension switch
			{
				".jpg" or ".jpeg" => "image/jpeg",
				".png" => "image/png",
				".gif" => "image/gif",
				".webp" => "image/webp",
				".bmp" => "image/bmp",
				_ => "image/jpeg" // Default to JPEG
			};
		}

		// Helper method to fix incorrectly nested tool arguments
		// Some models wrap arguments like: {"function_name": {...}} instead of just {...}
		private void FixToolCallArguments(ToolCall toolCall)
		{
			if (toolCall?.Function == null || string.IsNullOrWhiteSpace(toolCall.Function.Arguments))
				return;

			try
			{
				// Parse the arguments string to see what we have
				var argsNode = JsonNode.Parse(toolCall.Function.Arguments);

				if (argsNode is JsonObject argsObject && argsObject.Count == 1)
				{
					// Check if the single key matches the function name
					var firstKey = argsObject.First().Key;
					if (firstKey == toolCall.Function.Name)
					{
						// Extract the inner value and re-serialize it as the actual arguments
						var innerValue = argsObject[firstKey];
						toolCall.Function.Arguments = innerValue?.ToJsonString() ?? "{}";
						Debug.WriteLine($"Fixed nested tool arguments for {toolCall.Function.Name}");
					}
				}
			}
			catch (System.Text.Json.JsonException ex)
			{
				// If we can't parse the arguments, log it but don't throw
				Debug.WriteLine($"Could not parse tool arguments for fixing: {ex.Message}");
			}
		}

		// --- Helper methods for raising events ---

		// Notify subscribers when content is received (in streaming mode)
		private void RaiseContentReceived(string content)
		{
			//Debug.WriteLine($"Content received: {content}");
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

		// Notify subscribers when tool calls are received
		private void RaiseToolCallsReceived(List<ToolCall> toolCalls)
		{
			Debug.WriteLine($"Tool calls received: {toolCalls.Count} tool(s)");
			OnToolCallsReceived?.Invoke(this, toolCalls);
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

	 

}
