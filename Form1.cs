using LMStudioExampleFormApp.ApiClasses;
using LMStudioExampleFormApp.Interfaces;

using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace LMStudioExampleFormApp
{
    public partial class Form1 : Form
    {
        // Main AI client that handles communication with the LLM API
        private LMStudioExample _aiClient;

        // Cancellation token source to allow canceling requests
        private CancellationTokenSource? _cts;

        private List<ToolLMStudio> _tools { get; set; } = new();

        private void DefineAvailableToolsLMStudio()
        {
            var toolListPreview = new StringBuilder();

            // TOOL 1: Get Weather
            var getWeatherTool = new ToolBuilder()
                .AddToolName("get_weather")
                .AddDescription("Get the current weather for a specific city.")
                .AddProperty("city", "string", "The city to get the weather for.", isRequired: true)
                .Build();
            _tools.Add(getWeatherTool);

            toolListPreview.AppendLine(ToolStringOutputLMStudio.GenerateToolJson(getWeatherTool));

            // TOOL 2: Get Stock Price
            var getStockPriceTool = new ToolBuilder()
                .AddToolName("get_stock_price")
                .AddDescription("Gets the current stock price for a given stock ticker symbol.")
                .AddProperty("ticker_symbol", "string", "The stock ticker symbol (e.g., GOOG, MSFT).", isRequired: true)
                .Build();
            _tools.Add(getStockPriceTool);

            toolListPreview.AppendLine(ToolStringOutputLMStudio.GenerateToolJson(getStockPriceTool));

            Debug.WriteLine($"\nLMStudio Tools:\n{toolListPreview.ToString()}\n");
        }

        private bool streaming = false;

        public Form1()
        {
            // Initialize the form components defined in the designer
            InitializeComponent();

            _aiClient = new LMStudioExample(
                "http://localhost:1234/v1/chat/completions", // API endpoint URL
                "oss20",                             // Model to use
                                                     //"openai/gpt-oss-20b",                        // Model to use
                "you are a professional assistant"           // System instructions
            );

            DefineAvailableToolsLMStudio();

            _aiClient.initialize();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Set up event handlers when the form loads
            // These handle different stages of the AI response process
            _aiClient.OnContentReceived += AiClient_OnContentReceived; // Called when content chunks arrive
            _aiClient.OnComplete += AiClient_OnComplete;               // Called when the entire response is complete
            _aiClient.OnError += AiClient_OnError;                     // Called when an error occurs
            _aiClient.OnStatusUpdate += AiClient_OnStatusUpdate;       // Called when status changes
            _aiClient.OnToolCallsReceived += AiClient_OnToolCallsReceived; // Called when tool calls are received
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                await SendMessage(streaming: false, txtPrompt.Text.Trim());
            });
        }

        private async void btnSendNonStreaming_Click(object sender, EventArgs e)
        {
            // Text only (backward compatible)
            //     await _aiClient.SendMessageAsync("Hello, how are you?");

            // Text with single image
            //await _aiClient.SendMessageWithImagesAsync("What do you see in this image?",
            //    new[] { @"C:\Users\Administrator\Pictures\aug.jpg" });

            // Text with multiple images
            //   await _aiClient.SendMessageWithImagesAsync("Compare these images",
            //      new[] { @"C:\Users\Administrator\Pictures\cap.jpg", @"C:\Users\Administrator\Pictures\cap.png" });

            //// Non-streaming with images
            //await _aiClient.SendMessageWithImagesNonStreamingAsync("Analyze this photo and tell me what you see.",
            //    new[] { @"C:\Users\Administrator\Pictures\cap.png" });

            // Handle the streaming send button click
            // Note: This appears to be mislabeled - it's setting streaming to true
            await SendMessage(streaming: true);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Cancel the current request if there is one
            _cts?.Cancel();                  // Trigger cancellation
            btnSend.Enabled = true;          // Re-enable the send buttons
            btnSendNonStreaming.Enabled = true;
            Debug.WriteLine("Request canceled by user"); // Log cancellation
        }

        private async Task SendMessage(bool streaming, string? _userMessage = null)
        {
            // Get the user's message from the text box
            if (_userMessage == null) _userMessage = txtPrompt.Text.Trim();

            //if (string.IsNullOrEmpty(_userMessage))
            //    return; // Don't do anything if message is empty

            ChatMessage("User", $"{_userMessage}\n");

            if (streaming)
            {
                ChatMessage($"Assistant:");
            }

            await InvokeIfNeededAsync(async () => {
                 txtPrompt.Clear(); 
                // Update button states
                // btnCancel.Enabled = true;       // Enable cancel button
                btnSend.Enabled = false;        // Disable send buttons while processing
                btnSendNonStreaming.Enabled = false;            
            });



            // Cancel any existing request and create a new cancellation token
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            try
            {
                // Run the API call on a background thread to avoid UI freezing

                //if (streaming)
                //{
                //    _aiClient.SetTimeout(15);  // Set a short timeout for streaming requests
                //}
                //else
                //{
                //    _aiClient.SetTimeout(200); // Set a long timeout for non-streaming requests.
                //}

                var response = await _aiClient.SendMessageAsync(
                    userMessage: _userMessage,
                    imagePaths: null,
                    cancellationToken: _cts.Token,
                    tools: _tools);

                ChatMessage($"Assistant:{response}\n");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the request
                Debug.WriteLine($"Error sending message: {ex.Message}");

                await InvokeIfNeededAsync(async () => {
                txtResponse.Text = $"Error: {ex.Message}";
                // btnCancel.Enabled = false;
                });
            }
            finally
            {
                await InvokeIfNeededAsync(async () => {
                // Re-enable send buttons regardless of success or failure
                btnSend.Enabled = true;
                btnSendNonStreaming.Enabled = true;
                });

            }
        }

        private void ChatMessage(string user, string message)
        {
            InvokeIfNeeded(() => txtResponse.AppendText($"\r\n{user}: {message}"));
        }

        private void ChatMessage(string message)
        {
            InvokeIfNeeded(() => txtResponse.AppendText(message));
        }

        private void AiClient_OnStatusUpdate(object? sender, string e)
        {
            InvokeIfNeeded(() => this.Text = e);

            Debug.WriteLine($"Status updated: {e}");
        }

        private void AiClient_OnError(object? sender, Exception e)
        {
            InvokeIfNeeded(() =>
            {
                txtResponse.AppendText($"");
                // Display the error message in the response area
                ChatMessage($"\r\n\r\nError: {e.Message}");
                // Update UI state
                btnSend.Enabled = true;
                btnSendNonStreaming.Enabled = true;
                // btnCancel.Enabled = false;

                this.Text = "LMStudio Example";
            });

            Debug.WriteLine($"Error occurred: {e.Message}");
        }

        private void AiClient_OnComplete(object? sender, string e)
        {
            InvokeIfNeeded(() =>
            {
                // Update UI state
                btnSend.Enabled = true;
                btnSendNonStreaming.Enabled = true;
                // btnCancel.Enabled = false;
                _cts = null; // Clear the cancellation token
                this.Text = "LMStudio Example";
            });

            Debug.WriteLine("Response completed");
        }

        private void AiClient_OnContentReceived(object? sender, string e)
        {
            InvokeIfNeeded(() =>
            {
                // Enable the cancel button while content is streaming
                // btnCancel.Enabled = true;

                // Append the new content chunk to the response area
                txtResponse.AppendText(e);

                Debug.WriteLine($"Content received: {e}");
            });
        }

        private async void AiClient_OnToolCallsReceived(object? sender, List<ToolCall> e)
        {
            var aiResponse = new LMStudioResponse();

            try
            {
                foreach (var toolCall in e)
                {
                    if (toolCall.Function == null || toolCall.Function.Arguments == null) throw new Exception("Tool call arguments are null");
                }

                try
                {
                    try
                    {
                        foreach (var toolCall in e)
                        {
                            if (toolCall.Function == null) continue;

                            Debug.WriteLine($"=== TOOL CALL DEBUG START ===");
                            Debug.WriteLine($"Tool ID: {toolCall.Id}");
                            Debug.WriteLine($"Tool Name: {toolCall.Function.Name}");
                            Debug.WriteLine($"Raw Arguments JSON: {toolCall.Function.Arguments}");

                            // Execute the tool and get the result
                            string toolResult = ExecuteTool(toolCall.Function.Name, toolCall.Function.Arguments);

                            Debug.WriteLine($"Tool Result: {toolResult}");
                            Debug.WriteLine($"=== TOOL CALL DEBUG END ===");

                            // Add the tool result to the conversation
                            _aiClient.AddToolResult(toolCall.Id!, toolResult);
                        }

                        // Now send a follow-up request to get the AI's response based on the tool results
                        // Pass null for userMessage - the AI will see the tool results in the conversation history
                        await _aiClient.SendMessageAsync(null, tools: _tools);
                    }
                    catch (Exception jsonEx)
                    {
                        Debug.WriteLine($"Error deserializing tool arguments: {jsonEx.Message}");
                    }
                }
                catch (Exception finalEx)
                {
                    Debug.WriteLine($"Error in tool processing: {finalEx.Message}");
                }

                Debug.WriteLine("Response completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CRITICAL] LocalAiClient_OnToolCallsReceived error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Executes a tool call and returns simulated results
        /// </summary>
        private string ExecuteTool(string toolName, string argumentsJson)
        {
            try
            {
                switch (toolName)
                {
                    case "get_weather":
                        var weatherArgs = System.Text.Json.JsonSerializer.Deserialize<GetWeather>(argumentsJson);
                        return GetSimulatedWeather(weatherArgs?.City ?? "Unknown");

                    case "get_stock_price":
                        // Parse the ticker symbol from arguments
                        var stockArgs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(argumentsJson);
                        string ticker = stockArgs?.GetValueOrDefault("ticker_symbol", "UNKNOWN") ?? "UNKNOWN";
                        return GetSimulatedStockPrice(ticker);

                    default:
                        return System.Text.Json.JsonSerializer.Serialize(new
                        {
                            error = $"Unknown tool: {toolName}"
                        });
                }
            }
            catch (Exception ex)
            {
                return System.Text.Json.JsonSerializer.Serialize(new
                {
                    error = $"Tool execution error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Returns simulated weather data for a given city
        /// </summary>
        private string GetSimulatedWeather(string city)
        {
            // Generate some random-ish weather data based on city name hash
            var random = new Random(city.GetHashCode());
            int temp = random.Next(30, 85);
            string[] conditions = { "Sunny", "Partly cloudy", "Cloudy", "Rainy", "Snowy" };
            string condition = conditions[random.Next(conditions.Length)];
            int humidity = random.Next(40, 90);
            int windSpeed = random.Next(0, 25);

            var weatherData = new
            {
                city = city,
                temperature = temp,
                temperature_unit = "fahrenheit",
                conditions = condition,
                humidity = humidity,
                wind_speed = windSpeed,
                wind_unit = "mph",
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return System.Text.Json.JsonSerializer.Serialize(weatherData);
        }

        /// <summary>
        /// Returns simulated stock price data for a given ticker
        /// </summary>
        private string GetSimulatedStockPrice(string ticker)
        {
            // Generate some random-ish stock data based on ticker hash
            var random = new Random(ticker.GetHashCode());
            double price = random.Next(10, 500) + random.NextDouble();
            double change = (random.NextDouble() - 0.5) * 10;
            double changePercent = (change / price) * 100;

            var stockData = new
            {
                ticker = ticker,
                price = Math.Round(price, 2),
                change = Math.Round(change, 2),
                change_percent = Math.Round(changePercent, 2),
                volume = random.Next(1000000, 50000000),
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return System.Text.Json.JsonSerializer.Serialize(stockData);
        }

        private async Task LocalAiToolUseAsync(LMStudioResponse aiResponse, string requestId = "00000") // Added requestId parameter if needed
        {
            // Run on background thread to prevent blocking
            await Task.Run(async () =>
            {
                await ThreadHelper.InvokeOnUIThreadAsync(this, async () =>
                {
                    // DynamicIdeObject result = new(); // Local variable for tool results
                    StringBuilder toolResponseLog = new(); // Local StringBuilder for logging tool actions

                    try
                    {
                        var entry = new MessageLMStudio("Assistant") // Representing the AI's turn that led to the tool call
                        {
                            content = new List<IMessageContent>()
                        };

                        foreach (var item in aiResponse.content) // aiResponse here represents the *parsed* tool call info
                        {
                            if (item.Type == "tool_use") // Should only contain tool_use for this handler
                            {
                                entry.content.Add(new ToolUseContent
                                {
                                    Id = item.Id,       // This is the crucial tool_call_id from Local
                                    Name = item.Name,
                                    Input = item.Input // The parsed arguments
                                });
                                toolResponseLog.AppendLine($"Received Local tool use request: ID={item.Id}, Name={item.Name}");
                            }
                            else
                            {
                                // Log unexpected content types if they appear
                                toolResponseLog.AppendLine($"Warning: Unexpected content type '{item.Type}' in LocalToolUseAsync input.");
                            }
                        }

                        if (aiResponse?.content != null)
                        {
                            foreach (var item in aiResponse.content)
                            {
                                // Check if it's a valid tool_use content block
                                if (item != null && item.Type == "tool_use" && !string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.Name))
                                {
                                    // Use the new helper method to process the tool
                                    var toolMessage = await ProcessToolUse(item, toolResponseLog);

                                    // Log result for debugging
                                    string toolOutputString = string.Join("\n", toolMessage.result.dynamic_tool_result.output_content);
                                    toolResponseLog.AppendLine($"Prepared tool result for Local message history (Tool ID: {item.Id}). IsError: {toolMessage.result.dynamic_tool_result.is_error}");

                                    // Note: Add the tool result message to the list for the *next* Local API call
                                    // MessageListLocal.Add(MessageLocal.CreateToolResultMessage(item.id, toolOutputString));
                                } // End if (valid tool_use item)
                            } // End foreach (item in aiResponse.content)
                        } // End if (aiResponse content exists)
                    }
                    catch (Exception ex)
                    {
                        // Log error to UI/Debug
                        ChatMessage($"Error processing Local tool use: {ex.Message}");
                        //  await ErrorHandlerForm2.HandleError(ex, ErrorHandlerForm2.ErrorSeverity.High, "OpenAiToolUseAsync", this);
                        toolResponseLog.AppendLine($"EXCEPTION during Local tool processing: {ex.Message}"); // Log exception details
                    }
                    finally
                    {
                        // Log final debug info (ChatMessage handles marshalling)
                        ChatMessage($"Local Tool Use Processing Complete.");
                        // Log relevant info from aiResponse if needed (e.g., model used if available)
                        // await ChatMessage(ChatUser.Debug, $"Model:", $"{aiResponse.model}"); // If
                        // model info is part of your parsed response

                        // Optionally log the tool processing steps
                        Debug.WriteLine($"--- Local Tool Processing Log for Request {requestId} ---\n{toolResponseLog}");
                    }
                }); // End of InvokeOnUIThreadAsync
            }); // End Task.Run
            await Task.CompletedTask;
        }

        private async Task<(bool isAllowed, ToolResultObject result)> ProcessToolUse(Content item, StringBuilder toolResponseLog)
        {
            toolResponseLog.AppendLine($"Processing tool use: ID={item.Id}, Name={item.Name}");

            toolResponseLog.AppendLine($"Tool chain started for '{item.Name}'.");

            // Get the appropriate handler
            var handlerFunc = GetToolHandler(item.Name);

            toolResponseLog.AppendLine($"Executing handler for '{item.Name}'...");

            // Execute the handler
            ToolResultObject result;
            try
            {
                result = await handlerFunc(item);

                // Ensure there's at least one output message
                if (result.dynamic_tool_result.output_content == null || !result.dynamic_tool_result.output_content.Any())
                {
                    result.dynamic_tool_result.output_content = new List<string> { $"Operation completed: {item.Name}" };
                }
            }
            catch (Exception ex)
            {
                result = new ToolResultObject();
                result.dynamic_tool_result = new ToolResultContent
                {
                    is_error = true,
                    success = false,
                    output_content = new List<string> { $"Error executing {item.Name}: {ex.Message}" }
                };
                toolResponseLog.AppendLine($"Exception during handler execution: {ex.Message}");
            }

            toolResponseLog.AppendLine($"Handler execution completed for '{item.Name}'.");
            return (true, result);
        }

        /// <summary>
        /// Returns the appropriate tool handler function based on tool name.
        /// </summary>
        private Func<Content, Task<ToolResultObject>> GetToolHandler(string toolName)
        {
            switch (toolName)
            {
                // UIBridge Tools
                case "get_open_windows": return HandleUIBGetOpenWindows;

                default:
                    // Return a handler that generates an error for unknown tools
                    return async (contentItem) =>
                    {
                        var errorResult = new ToolResultObject();
                        errorResult.dynamic_tool_result = new ToolResultContent
                        {
                            is_error = true,
                            success = false,
                            output_content = new List<string> { $"Error: Unknown tool '{contentItem.Name}' was requested." }
                        };
                        return errorResult;
                    };
            }
        }

        private async Task<ToolResultObject> HandleUIBGetOpenWindows(Content? item)
        {
            ToolResultObject? result = new ToolResultObject();
            var toolResponse = new StringBuilder();

            try
            {
                toolResponse.AppendLine("\n--- Get Open Windows Request ---");

                string city = "Boston"; // Default value
                string unit = "fahrenheit"; // Default value

                city = item?.Input?.get_weather?.City ?? city;
                unit = item?.Input?.get_weather?.Unit ?? unit;

                var weatherResult = new GetWeather()
                {
                    City = city,
                    Temperature = new Random().Next(40, 90).ToString(), // Using random temp for mock
                    Forecast = "mostly sunny", // Using mock forecast
                    Unit = unit
                };

                var resultJson = JsonSerializer.Serialize(weatherResult, new JsonSerializerOptions { WriteIndented = false });

                _aiClient.AddToolResult(item.Id, resultJson);

                await SendMessage(true, "Done.");

                result.dynamic_tool_result.output_content.Add(toolResponse.ToString());
                result.dynamic_tool_result.success = true;
                result.dynamic_tool_result.is_error = false;
            }
            catch (Exception ex)
            {
                var errMsg = $"Failed to get open windows: {ex.Message}";
                ChatMessage(errMsg);
                Debug.WriteLine(errMsg);
                toolResponse.AppendLine(errMsg);
                result.dynamic_tool_result.output_content.Add(toolResponse.ToString());
                result.dynamic_tool_result.is_error = true;
            }
            finally
            {
                Debug.WriteLine(toolResponse.ToString());
                ChatMessage(toolResponse.ToString());
            }

            return result;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts?.Cancel();     // Cancel any pending requests
            _aiClient.Dispose(); // Dispose the AI client to release resources
        }

        private void txtResponse_TextChanged(object sender, EventArgs e)
        {
            txtResponse.SelectionStart = txtResponse.Text.Length;
            txtResponse.SelectionLength = 0;
            txtResponse.ScrollToCaret();
        }

        private async Task GetEmbedding()
        {
            try
            {
                string textToEmbed = txtPrompt.Text.Trim();
                if (string.IsNullOrEmpty(textToEmbed))
                {
                    MessageBox.Show("Please enter some text to embed");
                    return;
                }

                // Get embedding for a single text
                var embedding = await _aiClient.GetEmbeddingAsync(
                    textToEmbed,
                    "text-embedding-qwen3-embedding-0.6b"
                );

                if (embedding != null)
                {
                    txtResponse.AppendText($"Embedding generated successfully!\n");
                    txtResponse.AppendText($"Dimensions: {embedding.Length}\n");
                    //txtResponse.AppendText($"First 10 values: [{string.Join(", ", embedding.Take(10).Select(v => v.ToString("F6")))}...]\n");

                    txtResponse.AppendText($"[{string.Join(", ", embedding.Select(v => v.ToString("F6")))}...]\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting embedding: {ex.Message}");
                Debug.WriteLine($"Embedding error: {ex}");
            }
        }

        private async Task BatchEmbeddings()
        {
            await InvokeIfNeededAsync(async () =>
            {
                try
                {
                    // Example: Get embeddings for multiple texts at once
                    string[] textsToEmbed = new[]
                    {
                    "Machine learning is fascinating",
                    "Artificial intelligence is the future",
                    "Deep learning uses neural networks",
                    "Natural language processing enables chatbots"
                };

                    txtResponse.AppendText("Processing batch embeddings...\n\n");

                    var embeddings = await _aiClient.GetEmbeddingsBatchAsync(textsToEmbed);

                    if (embeddings != null)
                    {
                        txtResponse.AppendText($"Generated {embeddings.Length} embeddings\n\n");

                        for (int i = 0; i < textsToEmbed.Length; i++)
                        {
                            txtResponse.AppendText($"Text {i + 1}: \"{textsToEmbed[i]}\"\n");
                            txtResponse.AppendText($"  Embedding dimensions: {embeddings[i].Length}\n");
                            txtResponse.AppendText($"[{string.Join(", ", embeddings[i].Select(v => v.ToString("F4")))}...]\n\n");
                        }

                        // Calculate and display similarity matrix
                        txtResponse.AppendText("\nSimilarity Matrix:\n");
                        txtResponse.AppendText("       ");
                        for (int i = 0; i < embeddings.Length; i++)
                        {
                            txtResponse.AppendText($"Text{i + 1}  ");
                        }
                        txtResponse.AppendText("\n");

                        for (int i = 0; i < embeddings.Length; i++)
                        {
                            txtResponse.AppendText($"Text{i + 1}  ");
                            for (int j = 0; j < embeddings.Length; j++)
                            {
                                float similarity = LMStudioExample.CalculateCosineSimilarity(
                                    embeddings[i],
                                    embeddings[j]
                                );
                                txtResponse.AppendText($"{similarity:F3}   ");
                            }
                            txtResponse.AppendText("\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error with batch embeddings: {ex.Message}");
                    Debug.WriteLine($"Batch embeddings error: {ex}");
                }
            });
        }

        private async Task CompareSimilarity()
        {
            await InvokeIfNeededAsync(async () =>
            {
                try
                {
                    // Example: Compare similarity between two sentences
                    string text1 = "The cat sat on the mat";
                    string text2 = "A feline rested on the rug";
                    string text3 = "Python is a programming language";
                    txtResponse.AppendText("Calculating embeddings and similarity...\n\n");

                    // Get embeddings for all three texts
                    var embedding1 = await _aiClient.GetEmbeddingAsync(text1);
                    var embedding2 = await _aiClient.GetEmbeddingAsync(text2);
                    var embedding3 = await _aiClient.GetEmbeddingAsync(text3);

                    if (embedding1 != null && embedding2 != null && embedding3 != null)
                    {
                        // Calculate cosine similarities
                        float similarity1_2 = LMStudioExample.CalculateCosineSimilarity(embedding1, embedding2);
                        float similarity1_3 = LMStudioExample.CalculateCosineSimilarity(embedding1, embedding3);
                        float similarity2_3 = LMStudioExample.CalculateCosineSimilarity(embedding2, embedding3);

                        txtResponse.AppendText($"Text 1: \"{text1}\"\n");
                        txtResponse.AppendText($"Text 2: \"{text2}\"\n");
                        txtResponse.AppendText($"Text 3: \"{text3}\"\n\n");

                        txtResponse.AppendText($"Similarity between Text 1 and Text 2: {similarity1_2:F4}\n");
                        txtResponse.AppendText($"Similarity between Text 1 and Text 3: {similarity1_3:F4}\n");
                        txtResponse.AppendText($"Similarity between Text 2 and Text 3: {similarity2_3:F4}\n\n");

                        txtResponse.AppendText("Note: Values closer to 1.0 indicate higher similarity\n");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error comparing similarity: {ex.Message}");
                    Debug.WriteLine($"Similarity comparison error: {ex}");
                }
            });
        }

        private async Task SemanticSearch()
        {
            await InvokeIfNeededAsync(async () =>
            {
                try
                {
                    string query = txtPrompt.Text.Trim();
                    if (string.IsNullOrEmpty(query))
                    {
                        MessageBox.Show("Please enter a search query");
                        return;
                    }

                    // Sample document collection
                    string[] documents = new[]
                    {
                    "The quick brown fox jumps over the lazy dog",
                    "Machine learning models require training data",
                    "Python is a popular programming language",
                    "Natural language processing enables computers to understand text",
                    "The weather today is sunny and warm",
                    "Deep neural networks can learn complex patterns"
                };

                    txtResponse.AppendText($"Searching for: \"{query}\"\n\n");
                    txtResponse.AppendText("Processing documents...\n");

                    // Get embedding for query
                    var queryEmbedding = await _aiClient.GetEmbeddingAsync(query);

                    // Get embeddings for all documents
                    var documentEmbeddings = await _aiClient.GetEmbeddingsBatchAsync(documents);

                    if (queryEmbedding != null && documentEmbeddings != null)
                    {
                        // Calculate similarities and rank results
                        var results = new List<(string Document, float Similarity)>();

                        for (int i = 0; i < documents.Length; i++)
                        {
                            float similarity = LMStudioExample.CalculateCosineSimilarity(
                                queryEmbedding,
                                documentEmbeddings[i]
                            );
                            results.Add((documents[i], similarity));
                        }

                        // Sort by similarity (descending)
                        results = results.OrderByDescending(r => r.Similarity).ToList();

                        txtResponse.AppendText("\nSearch Results (ranked by relevance):\n\n");

                        for (int i = 0; i < results.Count; i++)
                        {
                            txtResponse.AppendText($"{i + 1}. [{results[i].Similarity:F4}] {results[i].Document}\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during semantic search: {ex.Message}");
                    Debug.WriteLine($"Semantic search error: {ex}");
                }
            });
        }

        private async Task ListAllModels()
        {
            await InvokeIfNeededAsync(async () =>
            {
                try
                {
                    txtResponse.Text = "Fetching all models...\n\n";

                    var models = await _aiClient.GetAllModelsAsync();

                    if (models != null && models.Length > 0)
                    {
                        txtResponse.AppendText($"Found {models.Length} total models:\n");
                        txtResponse.AppendText("=".PadRight(80, '=') + "\n\n");

                        foreach (var model in models)
                        {
                            txtResponse.AppendText($"Model ID: {model.Id}\n");
                            txtResponse.AppendText($"  Type: {model.Type}\n");
                            txtResponse.AppendText($"  Publisher: {model.Publisher}\n");
                            txtResponse.AppendText($"  Architecture: {model.Arch}\n");
                            txtResponse.AppendText($"  Quantization: {model.Quantization}\n");
                            txtResponse.AppendText($"  State: {model.State}\n");
                            txtResponse.AppendText($"  Max Context: {model.MaxContextLength:N0} tokens\n");
                            txtResponse.AppendText($"  Compatibility: {model.CompatibilityType}\n");
                            txtResponse.AppendText("\n");
                        }
                    }
                    else
                    {
                        txtResponse.AppendText("No models found.\n");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error listing models: {ex.Message}");
                    Debug.WriteLine($"List models error: {ex}");
                }
            });
        }

        private async Task ListLoadedModels()
        {
            await InvokeIfNeededAsync(async () =>
            {
                try
                {
                    txtResponse.Text = "Fetching loaded models...\n\n";

                    var loadedModels = await _aiClient.GetLoadedModelsAsync();

                    if (loadedModels != null && loadedModels.Length > 0)
                    {
                        txtResponse.AppendText($"Currently loaded models: {loadedModels.Length}\n");
                        txtResponse.AppendText("=".PadRight(80, '=') + "\n\n");

                        foreach (var model in loadedModels)
                        {
                            txtResponse.AppendText($"✓ {model.Id}\n");
                            txtResponse.AppendText($"  Type: {model.Type} | Quantization: {model.Quantization}\n");
                            txtResponse.AppendText($"  Max Context: {model.MaxContextLength:N0} tokens\n");
                            txtResponse.AppendText("\n");
                        }
                    }
                    else
                    {
                        txtResponse.AppendText("No models are currently loaded.\n");
                        txtResponse.AppendText("Load a model in LM Studio to use it.\n");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error listing loaded models: {ex.Message}");
                    Debug.WriteLine($"List loaded models error: {ex}");
                }
            });
        }

        private async Task GetModelInfo()
        {
            await InvokeIfNeededAsync(async () =>
            {
                try
                {
                    // Get model ID from user input (could be from txtPrompt or a separate textbox)
                    string modelId = txtPrompt.Text.Trim();

                    if (string.IsNullOrEmpty(modelId))
                    {
                        MessageBox.Show("Please enter a model ID in the prompt field.\nExample: qwen2-vl-7b-instruct");
                        return;
                    }

                    txtResponse.Text = $"Fetching info for model: {modelId}...\n\n";

                    var modelInfo = await _aiClient.GetModelInfoAsync(modelId);

                    if (modelInfo != null)
                    {
                        txtResponse.AppendText("Model Information\n");
                        txtResponse.AppendText("=".PadRight(80, '=') + "\n\n");

                        txtResponse.AppendText($"Model ID: {modelInfo.Id}\n");
                        txtResponse.AppendText($"Type: {modelInfo.Type}\n");
                        txtResponse.AppendText($"Publisher: {modelInfo.Publisher}\n");
                        txtResponse.AppendText($"Architecture: {modelInfo.Arch}\n");
                        txtResponse.AppendText($"Quantization: {modelInfo.Quantization}\n");
                        txtResponse.AppendText($"State: {modelInfo.State} {(modelInfo.IsLoaded ? "✓" : "✗")}\n");
                        txtResponse.AppendText($"Max Context Length: {modelInfo.MaxContextLength:N0} tokens\n");
                        txtResponse.AppendText($"Compatibility Type: {modelInfo.CompatibilityType}\n\n");

                        // Additional info based on model type
                        if (modelInfo.IsEmbeddingModel)
                        {
                            txtResponse.AppendText("📊 This is an embedding model for vector representations.\n");
                        }
                        else if (modelInfo.IsVisionModel)
                        {
                            txtResponse.AppendText("👁️ This is a vision-language model that can process images.\n");
                        }
                        else if (modelInfo.IsLanguageModel)
                        {
                            txtResponse.AppendText("💬 This is a text-only language model.\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error getting model info: {ex.Message}");
                    Debug.WriteLine($"Get model info error: {ex}");
                }
            });
        }

        private async Task ListModelsByType()
        {
            await InvokeIfNeededAsync(async () =>
            {
                try
                {
                    txtResponse.Text = "Categorizing models by type...\n\n";

                    // Get all models categorized by type
                    var embeddingModels = await _aiClient.GetEmbeddingModelsAsync();
                    var languageModels = await _aiClient.GetLanguageModelsAsync();
                    var visionModels = await _aiClient.GetVisionModelsAsync();

                    // Display Embedding Models
                    txtResponse.AppendText("📊 EMBEDDING MODELS\n");
                    txtResponse.AppendText("=".PadRight(80, '=') + "\n");
                    if (embeddingModels != null && embeddingModels.Length > 0)
                    {
                        foreach (var model in embeddingModels)
                        {
                            string status = model.IsLoaded ? "✓ LOADED" : "○ Not loaded";
                            txtResponse.AppendText($"{status} | {model.Id} ({model.Quantization})\n");
                        }
                    }
                    else
                    {
                        txtResponse.AppendText("No embedding models found.\n");
                    }
                    txtResponse.AppendText("\n");

                    // Display Language Models
                    txtResponse.AppendText("💬 LANGUAGE MODELS (LLMs)\n");
                    txtResponse.AppendText("=".PadRight(80, '=') + "\n");
                    if (languageModels != null && languageModels.Length > 0)
                    {
                        foreach (var model in languageModels)
                        {
                            string status = model.IsLoaded ? "✓ LOADED" : "○ Not loaded";
                            txtResponse.AppendText($"{status} | {model.Id} ({model.Quantization})\n");
                            txtResponse.AppendText($"         Context: {model.MaxContextLength:N0} tokens\n");
                        }
                    }
                    else
                    {
                        txtResponse.AppendText("No language models found.\n");
                    }
                    txtResponse.AppendText("\n");

                    // Display Vision Models
                    txtResponse.AppendText("👁️ VISION-LANGUAGE MODELS (VLMs)\n");
                    txtResponse.AppendText("=".PadRight(80, '=') + "\n");
                    if (visionModels != null && visionModels.Length > 0)
                    {
                        foreach (var model in visionModels)
                        {
                            string status = model.IsLoaded ? "✓ LOADED" : "○ Not loaded";
                            txtResponse.AppendText($"{status} | {model.Id} ({model.Quantization})\n");
                            txtResponse.AppendText($"         Context: {model.MaxContextLength:N0} tokens\n");
                        }
                    }
                    else
                    {
                        txtResponse.AppendText("No vision models found.\n");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error categorizing models: {ex.Message}");
                    Debug.WriteLine($"Categorize models error: {ex}");
                }
            });
        }

        private async Task CreateModelSelector()
        {
            await InvokeIfNeededAsync(async () =>
            {
                try
                {
                    // This example shows how to populate a ComboBox with available models
                    // Useful for letting users select which model to use

                    var models = await _aiClient.GetAllModelsAsync();

                    if (models != null && models.Length > 0)
                    {
                        // Create a form with a combo box to select models
                        Form modelSelectorForm = new Form
                        {
                            Text = "Select a Model",
                            Width = 500,
                            Height = 200,
                            StartPosition = FormStartPosition.CenterParent
                        };

                        ComboBox comboBox = new ComboBox
                        {
                            Left = 20,
                            Top = 20,
                            Width = 440,
                            DropDownStyle = ComboBoxStyle.DropDownList
                        };

                        // Populate combo box with models
                        foreach (var model in models)
                        {
                            string displayText = $"{model.Id} ({model.Type}, {model.State})";
                            comboBox.Items.Add(new { Text = displayText, Model = model });
                        }
                        comboBox.DisplayMember = "Text";
                        comboBox.SelectedIndex = 0;

                        Button selectButton = new Button
                        {
                            Text = "Select Model",
                            Left = 20,
                            Top = 60,
                            Width = 100
                        };

                        selectButton.Click += (s, ev) =>
                        {
                            if (comboBox.SelectedItem != null)
                            {
                                dynamic selectedItem = comboBox.SelectedItem;
                                ModelInfo selectedModel = selectedItem.Model;

                                txtResponse.Text = $"Selected Model: {selectedModel.Id}\n";
                                txtResponse.AppendText($"Type: {selectedModel.Type}\n");
                                txtResponse.AppendText($"State: {selectedModel.State}\n");
                                txtResponse.AppendText($"Max Context: {selectedModel.MaxContextLength:N0} tokens\n");

                                modelSelectorForm.Close();
                            }
                        };

                        modelSelectorForm.Controls.Add(comboBox);
                        modelSelectorForm.Controls.Add(selectButton);
                        modelSelectorForm.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("No models available.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating model selector: {ex.Message}");
                    Debug.WriteLine($"Model selector error: {ex}");
                }
            });
        }

        // Example: Check if a specific model type is available and loaded
        private async Task<bool> IsEmbeddingModelAvailable()
        {
            try
            {
                var embeddingModels = await _aiClient.GetEmbeddingModelsAsync();
                var loadedEmbeddingModel = embeddingModels?.FirstOrDefault(m => m.IsLoaded);

                if (loadedEmbeddingModel != null)
                {
                    Debug.WriteLine($"Embedding model available: {loadedEmbeddingModel.Id}");
                    return true;
                }
                else
                {
                    Debug.WriteLine("No embedding model is currently loaded");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking embedding model: {ex.Message}");
                return false;
            }
        }

        // Example: Validate that required models are loaded before operation
        private async Task SmartOperation()
        {
            await InvokeIfNeededAsync(async () =>
            {
                try
                {
                    txtResponse.Text = "Checking available models...\n\n";

                    var loadedModels = await _aiClient.GetLoadedModelsAsync();

                    if (loadedModels == null || loadedModels.Length == 0)
                    {
                        txtResponse.AppendText("❌ No models are loaded!\n");
                        txtResponse.AppendText("Please load a model in LM Studio first.\n");
                        return;
                    }

                    // Check for specific model types
                    bool hasLLM = loadedModels.Any(m => m.IsLanguageModel);
                    bool hasEmbedding = loadedModels.Any(m => m.IsEmbeddingModel);
                    bool hasVision = loadedModels.Any(m => m.IsVisionModel);

                    txtResponse.AppendText("Available capabilities:\n");
                    txtResponse.AppendText($"  {(hasLLM ? "✓" : "✗")} Text Generation (LLM)\n");
                    txtResponse.AppendText($"  {(hasEmbedding ? "✓" : "✗")} Embeddings\n");
                    txtResponse.AppendText($"  {(hasVision ? "✓" : "✗")} Vision Understanding (VLM)\n\n");

                    if (hasLLM)
                    {
                        txtResponse.AppendText("Ready to chat! You can ask questions.\n");
                    }

                    if (hasEmbedding)
                    {
                        txtResponse.AppendText("Ready for semantic search and embeddings!\n");
                    }

                    if (hasVision)
                    {
                        txtResponse.AppendText("Ready to analyze images!\n");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error checking models: {ex.Message}");
                    Debug.WriteLine($"Smart operation error: {ex}");
                }
            });
        }

        private async void embeddingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await GetEmbedding();
            await BatchEmbeddings();
            await CompareSimilarity();
            await SemanticSearch();
        }

        private async void visionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Send text with multiple images
            await _aiClient.SendMessageAsync(
                "What do you see?",
                new[] { @"C:\Users\Administrator\Pictures\aug.jpg", @"C:\Users\Administrator\Pictures\cap.jpg" }
            );

            // Send text with single image
            await _aiClient.SendMessageAsync(
                "What do you see?",
                new[] { @"C:\Users\Administrator\Pictures\aug.jpg" }
            );
        }

        private async void listAllModelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await ListAllModels();
        }

        private async void listLoadedModelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await ListLoadedModels();
        }

        private async void getModelInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await GetModelInfo();
        }

        private async void listModelsByTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await ListModelsByType();
        }

        private async void createModelSelectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await CreateModelSelector();
        }

        private async void isEmbeddingModelAvailableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var a = await IsEmbeddingModelAvailable();

            MessageBox.Show($"Is embedding model available? {a}");
        }

        private async void smartOperationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await SmartOperation();
        }

        private async Task<Task> InvokeIfNeededAsync(Func<Task> asyncAction)
        {
            return ThreadHelper.InvokeOnUIThreadAsync(this, asyncAction);
        }

        private void InvokeIfNeeded(Action action)
        {
            ThreadHelper.InvokeOnUIThread(this, action);
        }
    }
}