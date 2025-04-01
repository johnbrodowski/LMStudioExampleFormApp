namespace LMStudioExampleFormApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnSend = new Button();
            btnSendNonStreaming = new Button();
            btnCancel = new Button();
            txtResponse = new TextBox();
            txtPrompt = new TextBox();
            SuspendLayout();
            // 
            // btnSend
            // 
            btnSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSend.Location = new Point(508, 179);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(107, 23);
            btnSend.TabIndex = 0;
            btnSend.Text = "Send No Stream";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // btnSendNonStreaming
            // 
            btnSendNonStreaming.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSendNonStreaming.Location = new Point(508, 203);
            btnSendNonStreaming.Name = "btnSendNonStreaming";
            btnSendNonStreaming.Size = new Size(107, 23);
            btnSendNonStreaming.TabIndex = 1;
            btnSendNonStreaming.Text = "Send Streaming";
            btnSendNonStreaming.UseVisualStyleBackColor = true;
            btnSendNonStreaming.Click += btnSendNonStreaming_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Enabled = false;
            btnCancel.Location = new Point(508, 227);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(107, 23);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // txtResponse
            // 
            txtResponse.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtResponse.Location = new Point(4, 5);
            txtResponse.Multiline = true;
            txtResponse.Name = "txtResponse";
            txtResponse.Size = new Size(607, 169);
            txtResponse.TabIndex = 3;
            // 
            // txtPrompt
            // 
            txtPrompt.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtPrompt.Location = new Point(2, 180);
            txtPrompt.Multiline = true;
            txtPrompt.Name = "txtPrompt";
            txtPrompt.Size = new Size(500, 70);
            txtPrompt.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(616, 252);
            Controls.Add(txtPrompt);
            Controls.Add(txtResponse);
            Controls.Add(btnCancel);
            Controls.Add(btnSendNonStreaming);
            Controls.Add(btnSend);
            Name = "Form1";
            Text = "LMStudio Example";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSend;
        private Button btnSendNonStreaming;
        private Button btnCancel;
        private TextBox txtResponse;
        private TextBox txtPrompt;
    }
}
