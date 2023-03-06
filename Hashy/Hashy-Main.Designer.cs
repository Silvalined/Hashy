namespace Hashy
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.dirTextBox = new System.Windows.Forms.TextBox();
            this.scanButton = new System.Windows.Forms.Button();
            this.debugButton = new System.Windows.Forms.Button();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.sourceButton = new System.Windows.Forms.Button();
            this.outputButton = new System.Windows.Forms.Button();
            this.hashModeComboBox = new System.Windows.Forms.ComboBox();
            this.hashModeLabel = new System.Windows.Forms.Label();
            this.directoryToScanLabel = new System.Windows.Forms.Label();
            this.hashReportDestinationLabel = new System.Windows.Forms.Label();
            this.clearTerminalButton = new System.Windows.Forms.Button();
            this.totalProgressBar = new System.Windows.Forms.ProgressBar();
            this.checkButton = new System.Windows.Forms.Button();
            this.reportTextBox = new System.Windows.Forms.TextBox();
            this.totalPercentageLabel = new System.Windows.Forms.Label();
            this.consoleRichTextBox = new System.Windows.Forms.RichTextBox();
            this.existingReportLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // dirTextBox
            // 
            this.dirTextBox.Location = new System.Drawing.Point(12, 28);
            this.dirTextBox.Name = "dirTextBox";
            this.dirTextBox.Size = new System.Drawing.Size(399, 22);
            this.dirTextBox.TabIndex = 0;
            this.dirTextBox.Text = "E:\\deleteme\\source";
            // 
            // scanButton
            // 
            this.scanButton.Location = new System.Drawing.Point(336, 117);
            this.scanButton.Name = "scanButton";
            this.scanButton.Size = new System.Drawing.Size(75, 23);
            this.scanButton.TabIndex = 1;
            this.scanButton.Text = "Scan";
            this.scanButton.UseVisualStyleBackColor = true;
            this.scanButton.Click += new System.EventHandler(this.scanButton_Click);
            // 
            // debugButton
            // 
            this.debugButton.Location = new System.Drawing.Point(569, 96);
            this.debugButton.Name = "debugButton";
            this.debugButton.Size = new System.Drawing.Size(201, 98);
            this.debugButton.TabIndex = 3;
            this.debugButton.Text = "DEBUG!";
            this.debugButton.UseVisualStyleBackColor = true;
            this.debugButton.Click += new System.EventHandler(this.debugButton_Click);
            // 
            // outputTextBox
            // 
            this.outputTextBox.Location = new System.Drawing.Point(12, 72);
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.Size = new System.Drawing.Size(399, 22);
            this.outputTextBox.TabIndex = 4;
            this.outputTextBox.Text = "E:\\deleteme\\output.csv";
            // 
            // sourceButton
            // 
            this.sourceButton.Location = new System.Drawing.Point(417, 27);
            this.sourceButton.Name = "sourceButton";
            this.sourceButton.Size = new System.Drawing.Size(75, 23);
            this.sourceButton.TabIndex = 5;
            this.sourceButton.Text = "Browse";
            this.sourceButton.UseVisualStyleBackColor = true;
            this.sourceButton.Click += new System.EventHandler(this.sourceButton_Click);
            // 
            // outputButton
            // 
            this.outputButton.Location = new System.Drawing.Point(417, 71);
            this.outputButton.Name = "outputButton";
            this.outputButton.Size = new System.Drawing.Size(75, 23);
            this.outputButton.TabIndex = 6;
            this.outputButton.Text = "Browse";
            this.outputButton.UseVisualStyleBackColor = true;
            this.outputButton.Click += new System.EventHandler(this.outputButton_Click);
            // 
            // hashModeComboBox
            // 
            this.hashModeComboBox.FormattingEnabled = true;
            this.hashModeComboBox.Items.AddRange(new object[] {
            "MD5",
            "SHA256"});
            this.hashModeComboBox.Location = new System.Drawing.Point(12, 116);
            this.hashModeComboBox.Name = "hashModeComboBox";
            this.hashModeComboBox.Size = new System.Drawing.Size(121, 24);
            this.hashModeComboBox.TabIndex = 7;
            // 
            // hashModeLabel
            // 
            this.hashModeLabel.AutoSize = true;
            this.hashModeLabel.Location = new System.Drawing.Point(12, 97);
            this.hashModeLabel.Name = "hashModeLabel";
            this.hashModeLabel.Size = new System.Drawing.Size(80, 16);
            this.hashModeLabel.TabIndex = 8;
            this.hashModeLabel.Text = "Hash Mode:";
            // 
            // directoryToScanLabel
            // 
            this.directoryToScanLabel.AutoSize = true;
            this.directoryToScanLabel.Location = new System.Drawing.Point(12, 9);
            this.directoryToScanLabel.Name = "directoryToScanLabel";
            this.directoryToScanLabel.Size = new System.Drawing.Size(118, 16);
            this.directoryToScanLabel.TabIndex = 9;
            this.directoryToScanLabel.Text = "Directory To Scan:";
            // 
            // hashReportDestinationLabel
            // 
            this.hashReportDestinationLabel.AutoSize = true;
            this.hashReportDestinationLabel.Location = new System.Drawing.Point(12, 53);
            this.hashReportDestinationLabel.Name = "hashReportDestinationLabel";
            this.hashReportDestinationLabel.Size = new System.Drawing.Size(156, 16);
            this.hashReportDestinationLabel.TabIndex = 10;
            this.hashReportDestinationLabel.Text = "Hash Report Destination:";
            // 
            // clearTerminalButton
            // 
            this.clearTerminalButton.Location = new System.Drawing.Point(12, 171);
            this.clearTerminalButton.Name = "clearTerminalButton";
            this.clearTerminalButton.Size = new System.Drawing.Size(103, 23);
            this.clearTerminalButton.TabIndex = 11;
            this.clearTerminalButton.Text = "Clear Terminal";
            this.clearTerminalButton.UseVisualStyleBackColor = true;
            this.clearTerminalButton.Click += new System.EventHandler(this.clearTerminalButton_Click);
            // 
            // totalProgressBar
            // 
            this.totalProgressBar.Location = new System.Drawing.Point(12, 415);
            this.totalProgressBar.Name = "totalProgressBar";
            this.totalProgressBar.Size = new System.Drawing.Size(717, 23);
            this.totalProgressBar.TabIndex = 12;
            // 
            // checkButton
            // 
            this.checkButton.Location = new System.Drawing.Point(695, 71);
            this.checkButton.Name = "checkButton";
            this.checkButton.Size = new System.Drawing.Size(75, 23);
            this.checkButton.TabIndex = 13;
            this.checkButton.Text = "Check";
            this.checkButton.UseVisualStyleBackColor = true;
            this.checkButton.Click += new System.EventHandler(this.checkButton_Click);
            // 
            // reportTextBox
            // 
            this.reportTextBox.Location = new System.Drawing.Point(494, 27);
            this.reportTextBox.Name = "reportTextBox";
            this.reportTextBox.Size = new System.Drawing.Size(276, 22);
            this.reportTextBox.TabIndex = 14;
            this.reportTextBox.Text = "E:\\deleteme\\output.csv";
            // 
            // totalPercentageLabel
            // 
            this.totalPercentageLabel.AutoSize = true;
            this.totalPercentageLabel.Location = new System.Drawing.Point(730, 418);
            this.totalPercentageLabel.Name = "totalPercentageLabel";
            this.totalPercentageLabel.Size = new System.Drawing.Size(40, 16);
            this.totalPercentageLabel.TabIndex = 15;
            this.totalPercentageLabel.Text = "100%";
            this.totalPercentageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // consoleRichTextBox
            // 
            this.consoleRichTextBox.Location = new System.Drawing.Point(12, 200);
            this.consoleRichTextBox.Name = "consoleRichTextBox";
            this.consoleRichTextBox.ReadOnly = true;
            this.consoleRichTextBox.Size = new System.Drawing.Size(758, 209);
            this.consoleRichTextBox.TabIndex = 16;
            this.consoleRichTextBox.Text = "";
            this.consoleRichTextBox.TextChanged += new System.EventHandler(this.consoleRichTextBox_TextChanged);
            // 
            // existingReportLabel
            // 
            this.existingReportLabel.AutoSize = true;
            this.existingReportLabel.Location = new System.Drawing.Point(491, 9);
            this.existingReportLabel.Name = "existingReportLabel";
            this.existingReportLabel.Size = new System.Drawing.Size(100, 16);
            this.existingReportLabel.TabIndex = 17;
            this.existingReportLabel.Text = "Existing Report:";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(782, 453);
            this.Controls.Add(this.existingReportLabel);
            this.Controls.Add(this.consoleRichTextBox);
            this.Controls.Add(this.totalPercentageLabel);
            this.Controls.Add(this.reportTextBox);
            this.Controls.Add(this.checkButton);
            this.Controls.Add(this.totalProgressBar);
            this.Controls.Add(this.clearTerminalButton);
            this.Controls.Add(this.hashReportDestinationLabel);
            this.Controls.Add(this.directoryToScanLabel);
            this.Controls.Add(this.hashModeLabel);
            this.Controls.Add(this.hashModeComboBox);
            this.Controls.Add(this.outputButton);
            this.Controls.Add(this.sourceButton);
            this.Controls.Add(this.outputTextBox);
            this.Controls.Add(this.debugButton);
            this.Controls.Add(this.scanButton);
            this.Controls.Add(this.dirTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.Text = "Hashy - A file hash checker - By Silvalined © 2022 - PRE-RELEASE ALPHA";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox dirTextBox;
        private System.Windows.Forms.Button scanButton;
        private System.Windows.Forms.Button debugButton;
        private System.Windows.Forms.TextBox outputTextBox;
        private System.Windows.Forms.Button sourceButton;
        private System.Windows.Forms.Button outputButton;
        private System.Windows.Forms.ComboBox hashModeComboBox;
        private System.Windows.Forms.Label hashModeLabel;
        private System.Windows.Forms.Label directoryToScanLabel;
        private System.Windows.Forms.Label hashReportDestinationLabel;
        private System.Windows.Forms.Button clearTerminalButton;
        private System.Windows.Forms.ProgressBar totalProgressBar;
        private System.Windows.Forms.Button checkButton;
        private System.Windows.Forms.TextBox reportTextBox;
        private System.Windows.Forms.Label totalPercentageLabel;
        private System.Windows.Forms.RichTextBox consoleRichTextBox;
        private System.Windows.Forms.Label existingReportLabel;
    }
}

