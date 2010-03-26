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
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonClearSource = new System.Windows.Forms.Button();
            this.buttonClearDest = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxMinWaitTime = new System.Windows.Forms.TextBox();
            this.textBoxMaxWaitTime = new System.Windows.Forms.TextBox();
            this.labelInstructions = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxSourcePath
            // 
            this.textBoxSourcePath.Location = new System.Drawing.Point(51, 32);
            this.textBoxSourcePath.Name = "textBoxSourcePath";
            this.textBoxSourcePath.Size = new System.Drawing.Size(173, 20);
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
            this.buttonSourceBrowse.Location = new System.Drawing.Point(230, 30);
            this.buttonSourceBrowse.Name = "buttonSourceBrowse";
            this.buttonSourceBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonSourceBrowse.TabIndex = 2;
            this.buttonSourceBrowse.Text = "Browse";
            this.buttonSourceBrowse.UseVisualStyleBackColor = true;
            this.buttonSourceBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // buttonSourceAdd
            // 
            this.buttonSourceAdd.Location = new System.Drawing.Point(311, 30);
            this.buttonSourceAdd.Name = "buttonSourceAdd";
            this.buttonSourceAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonSourceAdd.TabIndex = 3;
            this.buttonSourceAdd.Text = "Add";
            this.buttonSourceAdd.UseVisualStyleBackColor = true;
            this.buttonSourceAdd.Click += new System.EventHandler(this.buttonSourceAdd_Click);
            // 
            // listBoxSourcePaths
            // 
            this.listBoxSourcePaths.AllowDrop = true;
            this.listBoxSourcePaths.FormattingEnabled = true;
            this.listBoxSourcePaths.Location = new System.Drawing.Point(16, 58);
            this.listBoxSourcePaths.Name = "listBoxSourcePaths";
            this.listBoxSourcePaths.Size = new System.Drawing.Size(450, 147);
            this.listBoxSourcePaths.TabIndex = 4;
            this.listBoxSourcePaths.DragDrop += new System.Windows.Forms.DragEventHandler(this.listBoxSourcePaths_DragDrop);
            this.listBoxSourcePaths.DragEnter += new System.Windows.Forms.DragEventHandler(this.listBox_DragEnter);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(81, 468);
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
            this.labelDuration.Location = new System.Drawing.Point(21, 445);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(86, 13);
            this.labelDuration.TabIndex = 6;
            this.labelDuration.Text = "Duration to Test:";
            // 
            // textBoxDuration
            // 
            this.textBoxDuration.Location = new System.Drawing.Point(113, 442);
            this.textBoxDuration.Name = "textBoxDuration";
            this.textBoxDuration.Size = new System.Drawing.Size(54, 20);
            this.textBoxDuration.TabIndex = 7;
            this.textBoxDuration.Text = "3600";
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
            this.listBoxDestPaths.AllowDrop = true;
            this.listBoxDestPaths.FormattingEnabled = true;
            this.listBoxDestPaths.Location = new System.Drawing.Point(16, 272);
            this.listBoxDestPaths.Name = "listBoxDestPaths";
            this.listBoxDestPaths.Size = new System.Drawing.Size(450, 147);
            this.listBoxDestPaths.TabIndex = 13;
            this.listBoxDestPaths.DragDrop += new System.Windows.Forms.DragEventHandler(this.listBoxDestPaths_DragDrop);
            this.listBoxDestPaths.DragEnter += new System.Windows.Forms.DragEventHandler(this.listBox_DragEnter);
            // 
            // buttonDestAdd
            // 
            this.buttonDestAdd.Location = new System.Drawing.Point(311, 243);
            this.buttonDestAdd.Name = "buttonDestAdd";
            this.buttonDestAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonDestAdd.TabIndex = 12;
            this.buttonDestAdd.Text = "Add";
            this.buttonDestAdd.UseVisualStyleBackColor = true;
            this.buttonDestAdd.Click += new System.EventHandler(this.buttonDestAdd_Click);
            // 
            // buttonDestBrowse
            // 
            this.buttonDestBrowse.Location = new System.Drawing.Point(230, 243);
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
            this.textBoxDest.Size = new System.Drawing.Size(173, 20);
            this.textBoxDest.TabIndex = 9;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Enabled = false;
            this.buttonCancel.Location = new System.Drawing.Point(243, 468);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(156, 23);
            this.buttonCancel.TabIndex = 15;
            this.buttonCancel.Text = "Cancel Test";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(16, 497);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(450, 23);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 16;
            // 
            // buttonClearSource
            // 
            this.buttonClearSource.Location = new System.Drawing.Point(391, 29);
            this.buttonClearSource.Name = "buttonClearSource";
            this.buttonClearSource.Size = new System.Drawing.Size(75, 23);
            this.buttonClearSource.TabIndex = 17;
            this.buttonClearSource.Text = "Clear";
            this.buttonClearSource.UseVisualStyleBackColor = true;
            this.buttonClearSource.Click += new System.EventHandler(this.buttonClearSource_Click);
            // 
            // buttonClearDest
            // 
            this.buttonClearDest.Location = new System.Drawing.Point(391, 243);
            this.buttonClearDest.Name = "buttonClearDest";
            this.buttonClearDest.Size = new System.Drawing.Size(75, 23);
            this.buttonClearDest.TabIndex = 18;
            this.buttonClearDest.Text = "Clear";
            this.buttonClearDest.UseVisualStyleBackColor = true;
            this.buttonClearDest.Click += new System.EventHandler(this.buttonClearDest_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(173, 445);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Min. Wait Time:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(319, 445);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Max. Wait Time:";
            // 
            // textBoxMinWaitTime
            // 
            this.textBoxMinWaitTime.Location = new System.Drawing.Point(259, 442);
            this.textBoxMinWaitTime.Name = "textBoxMinWaitTime";
            this.textBoxMinWaitTime.Size = new System.Drawing.Size(54, 20);
            this.textBoxMinWaitTime.TabIndex = 21;
            this.textBoxMinWaitTime.Text = "0";
            // 
            // textBoxMaxWaitTime
            // 
            this.textBoxMaxWaitTime.Location = new System.Drawing.Point(406, 442);
            this.textBoxMaxWaitTime.Name = "textBoxMaxWaitTime";
            this.textBoxMaxWaitTime.Size = new System.Drawing.Size(54, 20);
            this.textBoxMaxWaitTime.TabIndex = 22;
            this.textBoxMaxWaitTime.Text = "60";
            // 
            // labelInstructions
            // 
            this.labelInstructions.AutoSize = true;
            this.labelInstructions.Location = new System.Drawing.Point(13, 422);
            this.labelInstructions.Name = "labelInstructions";
            this.labelInstructions.Size = new System.Drawing.Size(123, 13);
            this.labelInstructions.TabIndex = 23;
            this.labelInstructions.Text = "Please enter in seconds:";
            // 
            // FormSeamlessTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(481, 537);
            this.Controls.Add(this.labelInstructions);
            this.Controls.Add(this.textBoxMaxWaitTime);
            this.Controls.Add(this.textBoxMinWaitTime);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonClearDest);
            this.Controls.Add(this.buttonClearSource);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonCancel);
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
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button buttonClearSource;
        private System.Windows.Forms.Button buttonClearDest;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxMinWaitTime;
        private System.Windows.Forms.TextBox textBoxMaxWaitTime;
        private System.Windows.Forms.Label labelInstructions;
    }
}

