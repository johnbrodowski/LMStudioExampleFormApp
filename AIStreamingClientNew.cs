using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LMStudio
{

    //Form usage
    /*


    using System;
using System.Threading;
using System.Windows.Forms;
using AIStreamingClient;

namespace AIFormApp
{
    public partial class MainForm : Form
    {
        private AIStreamingClient.AIStreamingClient _aiClient;
        private CancellationTokenSource _cts;

        public MainForm()
        {
            InitializeComponent();
            
            // Initialize the AI client
            _aiClient = new AIStreamingClient.AIStreamingClient(
                "http://localhost:1234/v1/chat/completions",
                "gemma-3-4b-it",
                "you are a professional assistant"
            );
            
            // Set up event handlers
            _aiClient.OnContentReceived += AiClient_OnContentReceived;
            _aiClient.OnComplete += AiClient_OnComplete;
            _aiClient.OnError += AiClient_OnError;
            _aiClient.OnStatusUpdate += AiClient_OnStatusUpdate;
        }

        private void AiClient_OnStatusUpdate(object sender, string e)
        {
            // In a form app, you need to invoke on the UI thread
            if (InvokeRequired)
            {
                Invoke(new Action(() => AiClient_OnStatusUpdate(sender, e)));
                return;
            }
            
            // Update status label
            lblStatus.Text = e;
            Debug.WriteLine($"Status updated: {e}");
        }

        private void AiClient_OnError(object sender, Exception e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AiClient_OnError(sender, e)));
                return;
            }
            
            // Show error to user
            txtResponse.AppendText($"\r\n\r\nError: {e.Message}");
            btnSend.Enabled = true;
            Debug.WriteLine($"Error occurred: {e.Message}");
        }

        private void AiClient_OnComplete(object sender, string e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AiClient_OnComplete(sender, e)));
                return;
            }
            
            // Update UI when response is complete
            txtResponse.AppendText("\r\n");
            btnSend.Enabled = true;
            _cts = null;
            Debug.WriteLine("Response completed");
        }

        private void AiClient_OnContentReceived(object sender, string e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AiClient_OnContentReceived(sender, e)));
                return;
            }
            
            // Add received content to the text box
            txtResponse.AppendText(e);
            Debug.WriteLine($"Content received: {e}");
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            string userMessage = txtPrompt.Text.Trim();
            if (string.IsNullOrEmpty(userMessage))
                return;
            
            // Reset UI for new response
            txtResponse.Clear();
            btnSend.Enabled = false;
            
            // Cancel any previous request
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            
            // Send the message
            await _aiClient.SendMessageAsync(userMessage, _cts.Token);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            btnSend.Enabled = true;
            Debug.WriteLine("Request canceled by user");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _cts?.Cancel();
            _aiClient.Dispose();
        }
    }
}



    */


    // Console Usage
    /*


        using System.Net.Http.Json;
    using System.Text.Json;
    using System.Text;
    using System.Net.Http;
    using System.Diagnostics;


    namespace LMStudio
    {
        internal class Program
        {
            // public static HttpClient httpClient = new HttpClient(); // Make httpClient static

            private static async Task Main(string[] args)
            {
                // Create a single HttpClient instance (best practice is to reuse it)
                // httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json"); // Remove this line
                // Create the AI client
                using (var aiClient = new AIStreamingClient(
                    "http://localhost:1234/v1/chat/completions",
                    "gemma-3-4b-it",
                    "you are a professional assistant"))
                {
                    // Set up event handlers
                    aiClient.OnContentReceived += (sender, content) =>
                    {
                        Debug.WriteLine($"Content received: {content}");
                        Console.Write(content); // Write to console as it arrives
                    };

                    aiClient.OnComplete += (sender, fullResponse) =>
                    {
                        Debug.WriteLine($"Complete! Full response ({fullResponse.Length} chars)");
                        Console.WriteLine("\nResponse complete.");
                    };

                    aiClient.OnError += (sender, ex) =>
                    {
                        Debug.WriteLine($"Error: {ex.Message}");
                        Console.WriteLine($"\nError occurred: {ex.Message}");
                    };

                    aiClient.OnStatusUpdate += (sender, status) =>
                    {
                        Debug.WriteLine($"Status: {status}");
                    };

                    // Example message
                    Console.WriteLine("Sending message: 'Explain how streaming works in APIs'");
                    await aiClient.SendMessageAsync("Explain how streaming works in APIs");

                    Console.WriteLine("\nPress any key to exit.");
                    Console.ReadKey();
                }

            }


        }
    }




        */

 
        public class AIStreamingClient : IDisposable
        {
            private readonly HttpClient _httpClient;
            private readonly string _endpoint;
            private readonly string _model;
            private readonly string _systemPrompt;
            private bool _disposed = false;

            // Events for streaming content and status updates
            public event EventHandler<string> OnContentReceived;
            public event EventHandler<string> OnComplete;
            public event EventHandler<Exception> OnError;
            public event EventHandler<string> OnStatusUpdate;

            public AIStreamingClient(string endpoint, string model, string systemPrompt)
            {
                _httpClient = new HttpClient();
                _endpoint = endpoint;
                _model = model;
                _systemPrompt = systemPrompt;
            }

            // Streaming version
            public async Task SendMessageAsync(string userMessage, CancellationToken cancellationToken = default)
            {
                if (string.IsNullOrEmpty(userMessage))
                    throw new ArgumentException("User message cannot be empty", nameof(userMessage));

                try
                {
                    RaiseStatusUpdate("Sending streaming request...");

                    var requestContent = new
                    {
                        model = _model,
                        messages = new[] {
                        new { role = "system", content = _systemPrompt },
                        new { role = "user", content = userMessage }
                    },
                        temperature = 0.7,
                        max_tokens = -1,
                        stream = true
                    };

                    var jsonContent = new StringContent(
                        JsonSerializer.Serialize(requestContent),
                        Encoding.UTF8,
                        "application/json");

                    var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
                    request.Content = jsonContent;

                    var response = await _httpClient.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken);

                    response.EnsureSuccessStatusCode();
                    RaiseStatusUpdate("Processing streaming response...");

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    {
                        string fullResponse = "";

                        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                        {
                            var line = await reader.ReadLineAsync();
                            if (string.IsNullOrEmpty(line))
                                continue;

                            if (line.StartsWith("data: "))
                            {
                                var jsonData = line.Substring(6).Trim();
                                if (jsonData == "[DONE]")
                                    break;

                                try
                                {
                                    var chunk = JsonSerializer.Deserialize<StreamingResponse>(jsonData);
                                    if (chunk?.choices != null && chunk.choices.Length > 0)
                                    {
                                        var choice = chunk.choices[0];
                                        if (choice.delta != null && !string.IsNullOrEmpty(choice.delta.content))
                                        {
                                            var content = choice.delta.content;
                                            fullResponse += content;
                                            RaiseContentReceived(content);
                                        }
                                    }
                                }
                                catch (JsonException ex)
                                {
                                    Debug.WriteLine($"JSON parsing error: {ex.Message}");
                                    Debug.WriteLine($"Problematic JSON: {jsonData}");
                                }
                            }
                        }

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            RaiseComplete(fullResponse);
                        }
                    }
                }
                catch (Exception ex)
                {
                    RaiseError(ex);
                }
            }

            // Non-streaming version
            public async Task<string> SendMessageNonStreamingAsync(string userMessage, CancellationToken cancellationToken = default)
            {
                if (string.IsNullOrEmpty(userMessage))
                    throw new ArgumentException("User message cannot be empty", nameof(userMessage));

                try
                {
                    RaiseStatusUpdate("Sending non-streaming request...");

                    var requestContent = new
                    {
                        model = _model,
                        messages = new[] {
                        new { role = "system", content = _systemPrompt },
                        new { role = "user", content = userMessage }
                    },
                        temperature = 0.7,
                        max_tokens = -1,
                        stream = false  // Non-streaming
                    };

                    var jsonContent = new StringContent(
                        JsonSerializer.Serialize(requestContent),
                        Encoding.UTF8,
                        "application/json");

                    var response = await _httpClient.PostAsync(_endpoint, jsonContent, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    RaiseStatusUpdate("Processing non-streaming response...");

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var aiResponse = JsonSerializer.Deserialize<AIMessage>(jsonResponse);

                    if (aiResponse?.choices != null && aiResponse.choices.Length > 0)
                    {
                        var responseContent = aiResponse.choices[0].message.content;
                        Debug.WriteLine($"Non-streaming response received: {responseContent.Length} characters");
                        RaiseComplete(responseContent);
                        return responseContent;
                    }
                    else
                    {
                        throw new Exception("Invalid response format");
                    }
                }
                catch (Exception ex)
                {
                    RaiseError(ex);
                    throw;
                }
            }

            // Event raising methods
            private void RaiseContentReceived(string content)
            {
                Debug.WriteLine($"Content received: {content}");
                OnContentReceived?.Invoke(this, content);
            }

            private void RaiseComplete(string fullResponse)
            {
                Debug.WriteLine($"Completed with full response of {fullResponse.Length} characters");
                OnComplete?.Invoke(this, fullResponse);
            }

            private void RaiseError(Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                OnError?.Invoke(this, ex);
            }

            private void RaiseStatusUpdate(string status)
            {
                Debug.WriteLine($"Status: {status}");
                OnStatusUpdate?.Invoke(this, status);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        _httpClient?.Dispose();
                    }
                    _disposed = true;
                }
            }
        }

        // Helper classes for parsing the streaming response
        public class StreamingResponse
        {
            public string id { get; set; }
            public string @object { get; set; }
            public int created { get; set; }
            public string model { get; set; }
            public StreamingChoice[] choices { get; set; }
        }

        public class StreamingChoice
        {
            public int index { get; set; }
            public DeltaMessage delta { get; set; }
            public string finish_reason { get; set; }
        }

        public class DeltaMessage
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        // Class for non-streaming response
        public class AIMessage
        {
            public string id { get; set; }
            public string @object { get; set; }
            public int created { get; set; }
            public string model { get; set; }
            public Choice[] choices { get; set; }
            public Usage usage { get; set; }
        }

        public class Choice
        {
            public int index { get; set; }
            public Message message { get; set; }
            public string finish_reason { get; set; }
        }

        public class Message
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        public class Usage
        {
            public int prompt_tokens { get; set; }
            public int completion_tokens { get; set; }
            public int total_tokens { get; set; }
        }
    }
