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
            txtPrompt = new TextBox();
            txtResponse = new RichTextBox();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            testsToolStripMenuItem = new ToolStripMenuItem();
            embeddingsToolStripMenuItem = new ToolStripMenuItem();
            getModelsToolStripMenuItem = new ToolStripMenuItem();
            visionToolStripMenuItem = new ToolStripMenuItem();
            modelsToolStripMenuItem = new ToolStripMenuItem();
            listAllModelsToolStripMenuItem = new ToolStripMenuItem();
            listLoadedModelsToolStripMenuItem = new ToolStripMenuItem();
            getModelInfoToolStripMenuItem = new ToolStripMenuItem();
            listModelsByTypeToolStripMenuItem = new ToolStripMenuItem();
            createModelSelectorToolStripMenuItem = new ToolStripMenuItem();
            isEmbeddingModelAvailableToolStripMenuItem = new ToolStripMenuItem();
            smartOperationToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btnSend
            // 
            btnSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSend.Location = new Point(467, 179);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(144, 23);
            btnSend.TabIndex = 0;
            btnSend.Text = "Send Streaming";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // btnSendNonStreaming
            // 
            btnSendNonStreaming.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSendNonStreaming.Location = new Point(467, 203);
            btnSendNonStreaming.Name = "btnSendNonStreaming";
            btnSendNonStreaming.Size = new Size(144, 23);
            btnSendNonStreaming.TabIndex = 1;
            btnSendNonStreaming.Text = "Send Non-Streaming";
            btnSendNonStreaming.UseVisualStyleBackColor = true;
            btnSendNonStreaming.Click += btnSendNonStreaming_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(467, 227);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(144, 23);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // txtPrompt
            // 
            txtPrompt.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtPrompt.Location = new Point(2, 180);
            txtPrompt.Multiline = true;
            txtPrompt.Name = "txtPrompt";
            txtPrompt.Size = new Size(459, 70);
            txtPrompt.TabIndex = 4;
            txtPrompt.Text = "Get the weather in boston.";
            // 
            // txtResponse
            // 
            txtResponse.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtResponse.Location = new Point(0, 27);
            txtResponse.Name = "txtResponse";
            txtResponse.Size = new Size(615, 147);
            txtResponse.TabIndex = 5;
            txtResponse.Text = "";
            txtResponse.TextChanged += txtResponse_TextChanged;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, testsToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(616, 24);
            menuStrip1.TabIndex = 7;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // testsToolStripMenuItem
            // 
            testsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { embeddingsToolStripMenuItem, getModelsToolStripMenuItem, visionToolStripMenuItem, modelsToolStripMenuItem });
            testsToolStripMenuItem.Name = "testsToolStripMenuItem";
            testsToolStripMenuItem.Size = new Size(45, 20);
            testsToolStripMenuItem.Text = "Tests";
            // 
            // embeddingsToolStripMenuItem
            // 
            embeddingsToolStripMenuItem.Name = "embeddingsToolStripMenuItem";
            embeddingsToolStripMenuItem.Size = new Size(140, 22);
            embeddingsToolStripMenuItem.Text = "Embeddings";
            embeddingsToolStripMenuItem.Click += embeddingsToolStripMenuItem_Click;
            // 
            // getModelsToolStripMenuItem
            // 
            getModelsToolStripMenuItem.Name = "getModelsToolStripMenuItem";
            getModelsToolStripMenuItem.Size = new Size(140, 22);
            getModelsToolStripMenuItem.Text = "Get Models";
            // 
            // visionToolStripMenuItem
            // 
            visionToolStripMenuItem.Name = "visionToolStripMenuItem";
            visionToolStripMenuItem.Size = new Size(140, 22);
            visionToolStripMenuItem.Text = "Vision";
            visionToolStripMenuItem.Click += visionToolStripMenuItem_Click;
            // 
            // modelsToolStripMenuItem
            // 
            modelsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { listAllModelsToolStripMenuItem, listLoadedModelsToolStripMenuItem, getModelInfoToolStripMenuItem, listModelsByTypeToolStripMenuItem, createModelSelectorToolStripMenuItem, isEmbeddingModelAvailableToolStripMenuItem, smartOperationToolStripMenuItem });
            modelsToolStripMenuItem.Name = "modelsToolStripMenuItem";
            modelsToolStripMenuItem.Size = new Size(140, 22);
            modelsToolStripMenuItem.Text = "Models";
            // 
            // listAllModelsToolStripMenuItem
            // 
            listAllModelsToolStripMenuItem.Name = "listAllModelsToolStripMenuItem";
            listAllModelsToolStripMenuItem.Size = new Size(225, 22);
            listAllModelsToolStripMenuItem.Text = "ListAllModels";
            listAllModelsToolStripMenuItem.Click += listAllModelsToolStripMenuItem_Click;
            // 
            // listLoadedModelsToolStripMenuItem
            // 
            listLoadedModelsToolStripMenuItem.Name = "listLoadedModelsToolStripMenuItem";
            listLoadedModelsToolStripMenuItem.Size = new Size(225, 22);
            listLoadedModelsToolStripMenuItem.Text = "ListLoadedModels";
            listLoadedModelsToolStripMenuItem.Click += listLoadedModelsToolStripMenuItem_Click;
            // 
            // getModelInfoToolStripMenuItem
            // 
            getModelInfoToolStripMenuItem.Name = "getModelInfoToolStripMenuItem";
            getModelInfoToolStripMenuItem.Size = new Size(225, 22);
            getModelInfoToolStripMenuItem.Text = "GetModelInfo";
            getModelInfoToolStripMenuItem.Click += getModelInfoToolStripMenuItem_Click;
            // 
            // listModelsByTypeToolStripMenuItem
            // 
            listModelsByTypeToolStripMenuItem.Name = "listModelsByTypeToolStripMenuItem";
            listModelsByTypeToolStripMenuItem.Size = new Size(225, 22);
            listModelsByTypeToolStripMenuItem.Text = "ListModelsByType";
            listModelsByTypeToolStripMenuItem.Click += listModelsByTypeToolStripMenuItem_Click;
            // 
            // createModelSelectorToolStripMenuItem
            // 
            createModelSelectorToolStripMenuItem.Name = "createModelSelectorToolStripMenuItem";
            createModelSelectorToolStripMenuItem.Size = new Size(225, 22);
            createModelSelectorToolStripMenuItem.Text = "CreateModelSelector";
            createModelSelectorToolStripMenuItem.Click += createModelSelectorToolStripMenuItem_Click;
            // 
            // isEmbeddingModelAvailableToolStripMenuItem
            // 
            isEmbeddingModelAvailableToolStripMenuItem.Name = "isEmbeddingModelAvailableToolStripMenuItem";
            isEmbeddingModelAvailableToolStripMenuItem.Size = new Size(225, 22);
            isEmbeddingModelAvailableToolStripMenuItem.Text = "IsEmbeddingModelAvailable";
            isEmbeddingModelAvailableToolStripMenuItem.Click += isEmbeddingModelAvailableToolStripMenuItem_Click;
            // 
            // smartOperationToolStripMenuItem
            // 
            smartOperationToolStripMenuItem.Name = "smartOperationToolStripMenuItem";
            smartOperationToolStripMenuItem.Size = new Size(225, 22);
            smartOperationToolStripMenuItem.Text = "SmartOperation";
            smartOperationToolStripMenuItem.Click += smartOperationToolStripMenuItem_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(616, 252);
            Controls.Add(txtResponse);
            Controls.Add(txtPrompt);
            Controls.Add(btnCancel);
            Controls.Add(btnSendNonStreaming);
            Controls.Add(btnSend);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "LMStudio Example";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSend;
        private Button btnSendNonStreaming;
        private Button btnCancel;
        private TextBox txtPrompt;
        private RichTextBox txtResponse;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem testsToolStripMenuItem;
        private ToolStripMenuItem embeddingsToolStripMenuItem;
        private ToolStripMenuItem getModelsToolStripMenuItem;
        private ToolStripMenuItem visionToolStripMenuItem;
        private ToolStripMenuItem modelsToolStripMenuItem;
        private ToolStripMenuItem listAllModelsToolStripMenuItem;
        private ToolStripMenuItem listLoadedModelsToolStripMenuItem;
        private ToolStripMenuItem getModelInfoToolStripMenuItem;
        private ToolStripMenuItem listModelsByTypeToolStripMenuItem;
        private ToolStripMenuItem createModelSelectorToolStripMenuItem;
        private ToolStripMenuItem isEmbeddingModelAvailableToolStripMenuItem;
        private ToolStripMenuItem smartOperationToolStripMenuItem;
    }
}
