namespace SynclessSeamlessTester
{
    partial class FormSeamlessTester
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
            this.textBoxSourcePath = new System.Windows.Forms.TextBox();
            this.labelBrowseSource = new System.Windows.Forms.Label();
            this.buttonSourceBrowse = new System.Windows.Forms.Button();
            this.buttonSourceAdd = new System.Windows.Forms.Button();
            this.listBoxSourcePaths = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.labelDuration = new System.Windows.Forms.Label();
            this.textBoxDuration = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.sourcePathsLabel = new System.Windows.Forms.Label();
            this.labelDestPaths = new System.Windows.Forms.Label();
            this.listBoxDestPaths = new System.Windows.Forms.ListBox();
            this.buttonDestAdd = new System.Windows.Forms.Button();
            this.buttonDestBrowse = new System.Windows.Forms.Button();
            this.labelDestPath = new System.Windows.Forms.Label();
            this.textBoxDest = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxSourcePath
            // 
            this.textBoxSourcePath.Location = new System.Drawing.Point(51, 32);
            this.textBoxSourcePath.Name = "textBoxSourcePath";
            this.textBoxSourcePath.Size = new System.Drawing.Size(253, 20);
            this.textBoxSourcePath.TabIndex = 0;
            // 
            // labelBrowseSource
            // 
            this.labelBrowseSource.AutoSize = true;
            this.labelBrowseSource.Location = new System.Drawing.Point(13, 35);
            this.labelBrowseSource.Name = "labelBrowseSource";
            this.labelBrowseSource.Size = new System.Drawing.Size(32, 13);
            this.labelBrowseSource.TabIndex = 1;
            this.labelBrowseSource.Text = "Path:";
            // 
            // buttonSourceBrowse
            // 
            this.buttonSourceBrowse.Location = new System.Drawing.Point(310, 30);
            this.buttonSourceBrowse.Name = "buttonSourceBrowse";
            this.buttonSourceBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonSourceBrowse.TabIndex = 2;
            this.buttonSourceBrowse.Text = "Browse";
            this.buttonSourceBrowse.UseVisualStyleBackColor = true;
            this.buttonSourceBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // buttonSourceAdd
            // 
            this.buttonSourceAdd.Location = new System.Drawing.Point(391, 30);
            this.buttonSourceAdd.Name = "buttonSourceAdd";
            this.buttonSourceAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonSourceAdd.TabIndex = 3;
            this.buttonSourceAdd.Text = "Add";
            this.buttonSourceAdd.UseVisualStyleBackColor = true;
            this.buttonSourceAdd.Click += new System.EventHandler(this.buttonSourceAdd_Click);
            // 
            // listBoxSourcePaths
            // 
            this.listBoxSourcePaths.FormattingEnabled = true;
            this.listBoxSourcePaths.Location = new System.Drawing.Point(16, 58);
            this.listBoxSourcePaths.Name = "listBoxSourcePaths";
            this.listBoxSourcePaths.Size = new System.Drawing.Size(450, 147);
            this.listBoxSourcePaths.TabIndex = 4;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(162, 458);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(156, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "Start Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // labelDuration
            // 
            this.labelDuration.AutoSize = true;
            this.labelDuration.Location = new System.Drawing.Point(95, 433);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(137, 13);
            this.labelDuration.TabIndex = 6;
            this.labelDuration.Text = "Duration to Test (Seconds):";
            // 
            // textBoxDuration
            // 
            this.textBoxDuration.Location = new System.Drawing.Point(238, 430);
            this.textBoxDuration.Name = "textBoxDuration";
            this.textBoxDuration.Size = new System.Drawing.Size(148, 20);
            this.textBoxDuration.TabIndex = 7;
            // 
            // sourcePathsLabel
            // 
            this.sourcePathsLabel.AutoSize = true;
            this.sourcePathsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sourcePathsLabel.Location = new System.Drawing.Point(13, 9);
            this.sourcePathsLabel.Name = "sourcePathsLabel";
            this.sourcePathsLabel.Size = new System.Drawing.Size(87, 13);
            this.sourcePathsLabel.TabIndex = 8;
            this.sourcePathsLabel.Text = "Source Paths:";
            // 
            // labelDestPaths
            // 
            this.labelDestPaths.AutoSize = true;
            this.labelDestPaths.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDestPaths.Location = new System.Drawing.Point(13, 223);
            this.labelDestPaths.Name = "labelDestPaths";
            this.labelDestPaths.Size = new System.Drawing.Size(111, 13);
            this.labelDestPaths.TabIndex = 14;
            this.labelDestPaths.Text = "Destination Paths:";
            // 
            // listBoxDestPaths
            // 
            this.listBoxDestPaths.FormattingEnabled = true;
            this.listBoxDestPaths.Location = new System.Drawing.Point(16, 272);
            this.listBoxDestPaths.Name = "listBoxDestPaths";
            this.listBoxDestPaths.Size = new System.Drawing.Size(450, 147);
            this.listBoxDestPaths.TabIndex = 13;
            // 
            // buttonDestAdd
            // 
            this.buttonDestAdd.Location = new System.Drawing.Point(391, 244);
            this.buttonDestAdd.Name = "buttonDestAdd";
            this.buttonDestAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonDestAdd.TabIndex = 12;
            this.buttonDestAdd.Text = "Add";
            this.buttonDestAdd.UseVisualStyleBackColor = true;
            this.buttonDestAdd.Click += new System.EventHandler(this.buttonDestAdd_Click);
            // 
            // buttonDestBrowse
            // 
            this.buttonDestBrowse.Location = new System.Drawing.Point(310, 244);
            this.buttonDestBrowse.Name = "buttonDestBrowse";
            this.buttonDestBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonDestBrowse.TabIndex = 11;
            this.buttonDestBrowse.Text = "Browse";
            this.buttonDestBrowse.UseVisualStyleBackColor = true;
            this.buttonDestBrowse.Click += new System.EventHandler(this.buttonDestBrowse_Click);
            // 
            // labelDestPath
            // 
            this.labelDestPath.AutoSize = true;
            this.labelDestPath.Location = new System.Drawing.Point(13, 249);
            this.labelDestPath.Name = "labelDestPath";
            this.labelDestPath.Size = new System.Drawing.Size(32, 13);
            this.labelDestPath.TabIndex = 10;
            this.labelDestPath.Text = "Path:";
            // 
            // textBoxDest
            // 
            this.textBoxDest.Location = new System.Drawing.Point(51, 246);
            this.textBoxDest.Name = "textBoxDest";
            this.textBoxDest.Size = new System.Drawing.Size(253, 20);
            this.textBoxDest.TabIndex = 9;
            // 
            // FormSeamlessTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(481, 520);
            this.Controls.Add(this.labelDestPaths);
            this.Controls.Add(this.listBoxDestPaths);
            this.Controls.Add(this.buttonDestAdd);
            this.Controls.Add(this.buttonDestBrowse);
            this.Controls.Add(this.labelDestPath);
            this.Controls.Add(this.textBoxDest);
            this.Controls.Add(this.sourcePathsLabel);
            this.Controls.Add(this.textBoxDuration);
            this.Controls.Add(this.labelDuration);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listBoxSourcePaths);
            this.Controls.Add(this.buttonSourceAdd);
            this.Controls.Add(this.buttonSourceBrowse);
            this.Controls.Add(this.labelBrowseSource);
            this.Controls.Add(this.textBoxSourcePath);
            this.Name = "FormSeamlessTester";
            this.Text = "Syncless - Seamless Tester";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxSourcePath;
        private System.Windows.Forms.Label labelBrowseSource;
        private System.Windows.Forms.Button buttonSourceBrowse;
        private System.Windows.Forms.Button buttonSourceAdd;
        private System.Windows.Forms.ListBox listBoxSourcePaths;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label labelDuration;
        private System.Windows.Forms.TextBox textBoxDuration;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Label sourcePathsLabel;
        private System.Windows.Forms.Label labelDestPaths;
        private System.Windows.Forms.ListBox listBoxDestPaths;
        private System.Windows.Forms.Button buttonDestAdd;
        private System.Windows.Forms.Button buttonDestBrowse;
        private System.Windows.Forms.Label labelDestPath;
        private System.Windows.Forms.TextBox textBoxDest;
    }
}

