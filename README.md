# LM Studio Example Form App 

A Windows Forms application that demonstrates how to integrate with LM Studio or other compatible AI services through a clean C# implementation.

## Overview

This application provides a simple user interface to interact with a local LM Studio API or any API compatible with the OpenAI Chat Completions endpoint. It demonstrates both streaming and non-streaming modes of communication with AI models.
 
## Features

- Connect to local LM Studio API or compatible endpoints
- Support for both streaming and non-streaming text generation
- Cancel ongoing requests
- Clean, well-documented codebase with comprehensive error handling
- Event-based architecture for real-time updates
- Proper resource management with IDisposable implementation

## Prerequisites

- .NET 9.0 or later
- LM Studio or compatible AI service running locally
- Basic understanding of C# and .NET Windows Forms

## Getting Started

### Setting Up LM Studio

1. Download and install [LM Studio](https://lmstudio.ai/)
2. Load a compatible model (e.g., Gemma, Llama, Mistral)
3. Start the local server (typically runs on http://localhost:1234)

### Configuring the Application

The application is pre-configured to connect to:
- URL: `http://localhost:1234/v1/chat/completions`
- Model: `gemma-3-4b-it`
- System prompt: `you are a professional assistant`

You can modify these parameters in the `Form1.cs` constructor:

```csharp
_aiClient = new LMStudioExample(
    "http://localhost:1234/v1/chat/completions", // API endpoint URL
    "gemma-3-4b-it",                             // Model to use
    "you are a professional assistant"           // System instructions
);
```

## Code Structure

The code is organized into two main components:

### 1. `LMStudioExample` Class

A reusable class that handles all communication with the AI API:

- `SendMessageAsync`: For streaming responses (content arrives in chunks)
- `SendMessageNonStreamingAsync`: For non-streaming responses (full response at once)
- Event-based notification system for:
  - `OnContentReceived`: When content chunks are received
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

## JSON Response Models

The application includes helper classes for deserializing JSON responses:

- For streaming responses:
  - `StreamingResponse`, `StreamingChoice`, `DeltaMessage`
  
- For non-streaming responses:
  - `AIMessage`, `Choice`, `Message`, `Usage`

## Error Handling

The application implements comprehensive error handling:
- Validation of user input
- Proper exception catching and reporting
- Graceful handling of request cancellation
- Clear error messages displayed to the user

## Resource Management

The `LMStudioExample` class implements `IDisposable` to ensure proper cleanup of resources, especially the `HttpClient` instance.

## How to Use

1. Enter your prompt in the text box
2. Click "Send (Non-Streaming)" for a complete response at once
3. Click "Send (Streaming)" to see the response arrive in real-time
4. Use the Cancel button to stop an ongoing request

## Customization

You can easily extend this application to:
- Add more UI elements for configuration
- Support authentication for commercial APIs
- Add conversation history
- Include advanced parameter controls (temperature, top_p, etc.)
- Implement chat-style multi-turn conversations

## Troubleshooting

Common issues:
- **Connection errors**: Ensure LM Studio server is running and accessible
- **Invalid model name**: Check that the specified model is loaded in LM Studio
- **Empty responses**: Verify the system prompt and user message are appropriate
- **Slow responses**: Consider using a smaller model or optimizing LM Studio settings

## License

[MIT License](LICENSE)

## Acknowledgments

- Uses standard .NET libraries for HTTP communication and JSON handling
- Compatible with LM Studio and other OpenAI-compatible API services

---

*This example application is provided for educational purposes. Use appropriate error handling and security measures for production applications.*
