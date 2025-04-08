using System.Diagnostics; // Import for Debug.WriteLine functionality

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
            _aiClient = new LMStudioExample(
                "http://localhost:1234/v1/chat/completions", // API endpoint URL
                "gemma-3-1b-it",                             // Model to use
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


            ChatMessage($"User:{userMessage}\n");

            if (streaming)
            {
                ChatMessage($"Assistant:");
            }

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
                        response = await _aiClient.SendMessageAsync(userMessage, _cts.Token);
                    }
                    else
                    {
                        // Use non-streaming mode - get the complete response at once
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
            txtResponse.AppendText("\r\n");

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

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Clean up resources when the form is closing
            base.OnFormClosing(e);
            _cts?.Cancel();     // Cancel any pending requests
            _aiClient.Dispose(); // Dispose the AI client to release resources
        }

        private void txtResponse_TextChanged(object sender, EventArgs e)
        {
            txtResponse.SelectionStart = txtResponse.Text.Length;
            txtResponse.SelectionLength = 0;
            txtResponse.ScrollToCaret();
        }
    }
}