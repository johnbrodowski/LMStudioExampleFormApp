# ATTENTION!!
This is the 'dev' branch and might not ever be merged with the master. I through this together this morning, so I would expect an issue or two. I added tool supprt using code from another project I'm working, so some things might appear out of place or have a name that doest exactly align with what you would expect. Also, you might run into something I have broken ('Cancel button') or an illeagl crossthreaded call or two; becuase something on the UI needs to be invoked and wasn't.

 

# LM Studio Example Form App 

A comprehensive Windows Forms application that demonstrates how to integrate with LM Studio or other compatible AI services through a clean C# implementation. This application supports text generation, vision models, embeddings, and model management.

## Overview

This application provides a feature-rich user interface to interact with a local LM Studio API or any API compatible with the OpenAI Chat Completions endpoint. It demonstrates streaming and non-streaming modes, multi-modal inputs (text + images), semantic search capabilities, and model discovery.

## Features

### Core Capabilities
- **Text Generation**: Connect to local LM Studio API or compatible endpoints
- **Streaming & Non-Streaming**: Support for both real-time and complete response modes
- **Vision Support**: Send images along with text prompts to vision-language models (VLMs)
- **Multi-Image Support**: Process multiple images in a single request
- **Conversation History**: Maintains context across multiple exchanges
- **Request Cancellation**: Cancel ongoing requests at any time

### Advanced Features
- **Embeddings API**: Generate vector embeddings for semantic search and similarity comparison
- **Model Discovery**: List and query all available models (loaded and downloaded)
- **Model Filtering**: Filter models by type (LLM, VLM, Embeddings)
- **Semantic Search**: Implement vector-based search using embeddings
- **Similarity Calculations**: Compare text similarity using cosine similarity

### Technical Features
- Clean, well-documented codebase with comprehensive error handling
- Event-based architecture for real-time updates
- Proper resource management with IDisposable implementation
- Polymorphic message content handling
- Thread-safe UI updates
- Configurable timeouts for different operation types

## Prerequisites

- .NET 9.0 or later
- LM Studio or compatible AI service running locally
- Basic understanding of C# and .NET Windows Forms

## Getting Started

### Setting Up LM Studio

1. Download and install [LM Studio](https://lmstudio.ai/)
2. Load a compatible model (e.g., Llama, Qwen, Mistral)
3. For vision capabilities, load a VLM model (e.g., qwen2-vl, llava)
4. For embeddings, load an embedding model (e.g., nomic-embed-text)
5. Start the local server (typically runs on http://localhost:1234)

### Configuring the Application

The application is pre-configured to connect to:
- URL: `http://localhost:1234/v1/chat/completions`
- Model: `lfm2-vl-1.6b`
- System prompt: `you are a professional assistant`

You can modify these parameters in the `Form1.cs` constructor:

```csharp
_aiClient = new LMStudioExample(
    "http://localhost:1234/v1/chat/completions", // API endpoint URL
    "lfm2-vl-1.6b",                              // Model to use
    "you are a professional assistant"           // System instructions
);
_aiClient.initialize();
```

## Code Structure

The code is organized into three main components:

### 1. `LMStudioExample` Class

A comprehensive class that handles all communication with the AI API:

#### Text Generation Methods:
- `SendMessageAsync(string userMessage)`: For streaming text responses
- `SendMessageNonStreamingAsync(string userMessage)`: For non-streaming text responses
- `SendMessageWithImagesAsync(string userMessage, string[] imagePaths)`: For streaming with image support
- `SendMessageWithImagesNonStreamingAsync(string userMessage, string[] imagePaths)`: For non-streaming with images

#### Embedding Methods:
- `GetEmbeddingAsync(string text, string embeddingModel)`: Get embedding vector for a single text
- `GetEmbeddingsBatchAsync(string[] texts, string embeddingModel)`: Get embeddings for multiple texts
- `CalculateCosineSimilarity(float[] embedding1, float[] embedding2)`: Compare two embeddings (static method)

#### Model Discovery Methods:
- `GetAllModelsAsync()`: Retrieve all available models (loaded and downloaded)
- `GetModelInfoAsync(string modelId)`: Get detailed information about a specific model
- `GetLoadedModelsAsync()`: Get only the models currently loaded in memory
- `GetEmbeddingModelsAsync()`: Get all embedding models
- `GetLanguageModelsAsync()`: Get all language models (LLMs)
- `GetVisionModelsAsync()`: Get all vision-language models (VLMs)

#### Utility Methods:
- `SetTimeout(long timeSpanInSeconds)`: Configure request timeout
- `initialize(long timeoutInSeconds)`: Initialize the client with conversation history

#### Event-based Notification System:
- `OnContentReceived`: When content chunks are received (streaming mode)
- `OnComplete`: When a response is complete
- `OnStatusUpdate`: For status changes
- `OnError`: When errors occur

### 2. `Form1` Class

The Windows Forms UI that:
- Creates and configures the LMStudioExample instance
- Handles user input and button clicks
- Updates UI elements based on response events
- Manages request cancellation
- Ensures UI updates occur on the correct thread
- Maintains chat history display

### 3. Message and Content Classes

#### Message Content System:
- `IMessageContent`: Interface for polymorphic message content
- `TextContent`: Text-based message content
- `ImageContent`: Image-based message content with base64 encoding
- `ImageUrlData`: Container for image URLs (data URLs with base64)

#### Message Factory Methods:
- `Message.CreateSystemMessage(string text)`: Create system instruction messages
- `Message.CreateUserTextMessage(string text)`: Create user text messages
- `Message.CreateAssistantMessage(string text)`: Create assistant response messages

#### Custom JSON Converters:
- `MessageContentConverter`: Handles polymorphic serialization of message content
- `FlexibleContentConverter`: Handles both string and array content formats

## JSON Response Models

The application includes comprehensive classes for deserializing various API responses:

### For Chat Completions:
- **Streaming responses**: `StreamingResponse`, `StreamingChoice`, `DeltaMessage`
- **Non-streaming responses**: `AIMessage`, `Choice`, `Message`, `Usage`

### For Embeddings:
- `EmbeddingResponse`: Container for embedding results
- `EmbeddingData`: Individual embedding with vector and index
- `EmbeddingUsage`: Token usage information
- `EmbeddingRequest`: Request format for embeddings API

### For Model Discovery:
- `ModelsListResponse`: Container for list of models
- `ModelInfo`: Detailed information about a model including:
  - Model ID, type, publisher, architecture
  - Quantization method, state (loaded/not-loaded)
  - Max context length, compatibility type
  - Helper properties: `IsLoaded`, `IsEmbeddingModel`, `IsLanguageModel`, `IsVisionModel`

## Image Support

The application supports multiple image formats:
- JPEG (.jpg, .jpeg)
- PNG (.png)
- GIF (.gif)
- WebP (.webp)
- BMP (.bmp)

Images are automatically:
- Read from disk
- Converted to base64 encoding
- Embedded in data URLs with proper MIME types
- Included in the message content array

### Example Usage:
```csharp
// Single image
await _aiClient.SendMessageWithImagesAsync(
    "What do you see in this image?",
    new[] { @"C:\path\to\image.jpg" }
);

// Multiple images
await _aiClient.SendMessageWithImagesAsync(
    "Compare these images",
    new[] { @"C:\path\to\image1.jpg", @"C:\path\to\image2.png" }
);
```

## Embeddings and Semantic Search

### Generating Embeddings:
```csharp
// Single text embedding
var embedding = await _aiClient.GetEmbeddingAsync(
    "Machine learning is fascinating",
    "text-embedding-nomic-embed-text-v1.5"
);

// Batch embeddings
var embeddings = await _aiClient.GetEmbeddingsBatchAsync(
    new[] { "Text 1", "Text 2", "Text 3" },
    "text-embedding-nomic-embed-text-v1.5"
);
```

### Calculating Similarity:
```csharp
float similarity = LMStudioExample.CalculateCosineSimilarity(embedding1, embedding2);
// Returns value between -1 and 1 (1 = identical, 0 = unrelated)
```

### Implementing Semantic Search:
```csharp
// 1. Get query embedding
var queryEmbedding = await _aiClient.GetEmbeddingAsync(query);

// 2. Get document embeddings
var docEmbeddings = await _aiClient.GetEmbeddingsBatchAsync(documents);

// 3. Calculate similarities and rank
var results = documents
    .Select((doc, i) => new {
        Document = doc,
        Similarity = LMStudioExample.CalculateCosineSimilarity(
            queryEmbedding, 
            docEmbeddings[i]
        )
    })
    .OrderByDescending(r => r.Similarity)
    .ToList();
```

## Model Discovery and Management

### List All Models:
```csharp
var models = await _aiClient.GetAllModelsAsync();
foreach (var model in models)
{
    Console.WriteLine($"{model.Id} - {model.Type} ({model.State})");
}
```

### Check Loaded Models:
```csharp
var loadedModels = await _aiClient.GetLoadedModelsAsync();
if (loadedModels?.Length > 0)
{
    // Models are ready to use
}
```

### Filter by Type:
```csharp
var embeddingModels = await _aiClient.GetEmbeddingModelsAsync();
var languageModels = await _aiClient.GetLanguageModelsAsync();
var visionModels = await _aiClient.GetVisionModelsAsync();
```

### Get Specific Model Info:
```csharp
var modelInfo = await _aiClient.GetModelInfoAsync("qwen2-vl-7b-instruct");
Console.WriteLine($"Max Context: {modelInfo.MaxContextLength} tokens");
Console.WriteLine($"Loaded: {modelInfo.IsLoaded}");
```

## Error Handling

The application implements comprehensive error handling:
- Validation of user input (empty messages, missing files)
- File existence checks for images
- Proper exception catching and reporting
- Graceful handling of request cancellation
- HTTP status code validation
- JSON parsing error handling
- Clear error messages displayed to the user
- Debug logging for troubleshooting

## Resource Management

The `LMStudioExample` class implements `IDisposable` to ensure proper cleanup of resources, especially the `HttpClient` instance. Always dispose of the client when done:

```csharp
protected override void OnFormClosing(FormClosingEventArgs e)
{
    _cts?.Cancel();
    _aiClient.Dispose();
    base.OnFormClosing(e);
}
```

## How to Use

### Basic Text Chat:
1. Enter your prompt in the text box
2. Click "Send (Non-Streaming)" for a complete response at once
3. Click "Send (Streaming)" to see the response arrive in real-time
4. Use the Cancel button to stop an ongoing request

### With Images:
1. Modify the code to include image paths
2. Call `SendMessageWithImagesAsync()` with your text and image paths
3. Ensure you're using a VLM model that supports vision

### Embeddings:
1. Load an embedding model in LM Studio
2. Call `GetEmbeddingAsync()` with your text
3. Use the resulting vectors for similarity comparisons or search

### Model Management:
1. Call `GetAllModelsAsync()` to see available models
2. Use `GetLoadedModelsAsync()` to check what's ready
3. Filter by type to find specific model capabilities

## Customization

You can easily extend this application to:
- Add UI elements for model selection dropdowns
- Implement image upload functionality through file dialogs
- Create a vector database for storing embeddings
- Build a full semantic search interface
- Add support for authentication tokens
- Include advanced parameter controls (temperature, top_p, max_tokens)
- Implement persistent conversation history
- Add support for function calling
- Create batch processing workflows
- Build a RAG (Retrieval Augmented Generation) system

## API Endpoints Used

- **Chat Completions**: `http://localhost:1234/v1/chat/completions`
- **Embeddings**: `http://localhost:1234/api/v0/embeddings`
- **Models List**: `http://localhost:1234/api/v0/models`
- **Specific Model**: `http://localhost:1234/api/v0/models/{modelId}`

## Troubleshooting

### Common Issues:

**Connection errors**: 
- Ensure LM Studio server is running and accessible
- Check firewall settings
- Verify the endpoint URL is correct

**Invalid model name**: 
- Check that the specified model is loaded in LM Studio
- Use `GetAllModelsAsync()` to see available models
- Ensure model ID matches exactly (case-sensitive)

**Empty responses**: 
- Verify the system prompt and user message are appropriate
- Check if the model is actually loaded (`GetLoadedModelsAsync()`)
- Review timeout settings

**Image not found errors**:
- Verify image file paths are correct and absolute
- Check file permissions
- Ensure image format is supported

**Slow responses**: 
- Consider using a smaller model
- Optimize LM Studio settings (GPU layers, context size)
- Increase timeout for non-streaming requests
- Use streaming mode for better perceived performance

**Embedding dimension mismatch**:
- Ensure you're using the same embedding model for all texts being compared
- Different models produce different dimension vectors

**Model not loaded errors**:
- Load the required model type in LM Studio before use
- Check model state with `GetModelInfoAsync()`
- VLMs required for image processing
- Embedding models required for embeddings

## Performance Considerations

- **Streaming vs Non-Streaming**: Use streaming for better user experience on longer responses
- **Timeout Settings**: Set shorter timeouts (15s) for streaming, longer (200s) for non-streaming
- **Batch Embeddings**: Process multiple texts together when possible
- **Image Size**: Larger images increase processing time and memory usage
- **Context Length**: Monitor token usage against model's max context length
- **Model Size**: Smaller quantized models (Q4, Q5) are faster but may have lower quality

## License

[MIT License](LICENSE)

## Acknowledgments

- Uses standard .NET libraries for HTTP communication and JSON handling
- Compatible with LM Studio and other OpenAI-compatible API services
- Supports OpenAI Chat Completions API format
- Follows LM Studio's API conventions for embeddings and model discovery

---

*This example application is provided for educational purposes. Use appropriate error handling and security measures for production applications. Always validate user input and sanitize file paths before processing.*
