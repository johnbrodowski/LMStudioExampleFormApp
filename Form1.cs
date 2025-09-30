using System.Diagnostics;
using System.Text.Json; // Import for Debug.WriteLine functionality

namespace LMStudioExampleFormApp
{
    public partial class Form1 : Form
    {
        // Main AI client that handles communication with the LLM API
        private LMStudioExample _aiClient;

        // Cancellation token source to allow canceling requests
        private CancellationTokenSource? _cts;



        public Form1()
        {
            // Initialize the form components defined in the designer
            InitializeComponent();
 
            // Create the AI client with endpoint URL, model name, and system prompt
            //_aiClient = new LMStudioExampleFormApp.two.LMStudioExample(
            _aiClient = new LMStudioExample(
                "http://localhost:1234/v1/chat/completions", // API endpoint URL
                "lfm2-vl-1.6b",                             // Model to use
                                                            //"openai/gpt-oss-20b",                        // Model to use
                "you are a professional assistant"           // System instructions
            );

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
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            // Handle the non-streaming send button click

            await SendMessage(streaming: false);
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

        private async Task SendMessage(bool streaming)
        {
            // Get the user's message from the text box
            string userMessage = txtPrompt.Text.Trim();
            if (string.IsNullOrEmpty(userMessage))
                return; // Don't do anything if message is empty


            ChatMessage($"\nUser:{userMessage}\n");

            // if (streaming)
            // {
            ChatMessage($"Assistant:");
            // }

            string response = "";
            // Clear the UI elements for the new response
            txtPrompt.Clear();
            // txtResponse.Clear();

            // Update button states
            btnCancel.Enabled = true;       // Enable cancel button
            btnSend.Enabled = false;        // Disable send buttons while processing
            btnSendNonStreaming.Enabled = false;

            // Cancel any existing request and create a new cancellation token
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            try
            {
                // Run the API call on a background thread to avoid UI freezing
                await Task.Run(async () =>
                {
                    if (streaming)
                    {
                        // Use streaming mode - content will come in chunks via events
                        _aiClient.SetTimeout(15);  // Set a short timeout for streaming requests
                        response = await _aiClient.SendMessageAsync(userMessage, _cts.Token);
                    }
                    else
                    {
                        // Use non-streaming mode - get the complete response at once
                        _aiClient.SetTimeout(200); // Set a long timeout for non-streaming requests.
                        response = await _aiClient.SendMessageNonStreamingAsync(userMessage, _cts.Token);
                        ChatMessage($"Assistant:{response}\n");
                        // Update the UI with the full response
                    }
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the request
                Debug.WriteLine($"Error sending message: {ex.Message}");
                txtResponse.Text = $"Error: {ex.Message}";
                btnCancel.Enabled = false;
            }
            finally
            {
                // Re-enable send buttons regardless of success or failure
                btnSend.Enabled = true;
                btnSendNonStreaming.Enabled = true;
            }
        }

        private void ChatMessage(string Message)
        {
            // Helper method to update the response text box from any thread
            if (InvokeRequired)
            {
                // If called from a non-UI thread, use Invoke to switch to the UI thread
                Invoke(new Action(() => ChatMessage(Message)));
                return;
            }

            // Update the text box with the complete message
            txtResponse.Text += $"{Message}";
            Debug.WriteLine($"ChatMessage: {Message}");
        }

        private void AiClient_OnStatusUpdate(object? sender, string e)
        {
            // Handle status update events from the AI client
            if (InvokeRequired)
            {
                // Ensure we're on the UI thread
                Invoke(new Action(() => AiClient_OnStatusUpdate(sender, e)));
                return;
            }

            // Update the form title with the status message
            this.Text = e;
            Debug.WriteLine($"Status updated: {e}");
        }

        private void AiClient_OnError(object? sender, Exception e)
        {
            // Handle error events from the AI client
            if (InvokeRequired)
            {
                // Ensure we're on the UI thread
                Invoke(new Action(() => AiClient_OnError(sender, e)));
                return;
            }

            // Display the error message in the response area
            txtResponse.AppendText($"\r\n\r\nError: {e.Message}");

            // Update UI state
            btnSend.Enabled = true;
            btnSendNonStreaming.Enabled = true;
            btnCancel.Enabled = false;

            Debug.WriteLine($"Error occurred: {e.Message}");
            this.Text = "LMStudio Example";
        }
 
        private void AiClient_OnComplete(object? sender, string e)
        {
            // Handle completion events from the AI client
            if (InvokeRequired)
            {
                // Ensure we're on the UI thread
                Invoke(new Action(() => AiClient_OnComplete(sender, e)));
                return;
            }

            // Add a new line to the response area
            // txtResponse.AppendText("\r\n");

            // Update UI state
            btnSend.Enabled = true;
            btnSendNonStreaming.Enabled = true;
            btnCancel.Enabled = false;
            _cts = null; // Clear the cancellation token

            Debug.WriteLine("Response completed");
            this.Text = "LMStudio Example";
        }

        private void AiClient_OnContentReceived(object? sender, string e)
        {
            // Handle content received events from the AI client (streaming mode)
            if (InvokeRequired)
            {
                // Ensure we're on the UI thread
                Invoke(new Action(() => AiClient_OnContentReceived(sender, e)));
                return;
            }

            // Enable the cancel button while content is streaming
            btnCancel.Enabled = true;

            // Append the new content chunk to the response area
            txtResponse.AppendText(e);

            Debug.WriteLine($"Content received: {e}");
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
                        //txtResponse.AppendText($"  First 5 values: [{string.Join(", ", embeddings[i].Take(5).Select(v => v.ToString("F4")))}...]\n\n");
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
        }

        private async Task CompareSimilarity()
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
        }

        private async Task SemanticSearch()
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
        }
 
        private async Task ListAllModels()
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
        }

        private async Task ListLoadedModels()
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
        }

        private async Task GetModelInfo()
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
        }

        private async Task ListModelsByType()
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
        }

        private async Task CreateModelSelector()
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
            await _aiClient.SendMessageWithImagesAsync(
                "What do you see?",
                new[] { @"C:\Users\Administrator\Pictures\aug.jpg", @"C:\Users\Administrator\Pictures\cap.jpg" }
            );


            // Send text with single image
            await _aiClient.SendMessageWithImagesAsync(
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
    }
}