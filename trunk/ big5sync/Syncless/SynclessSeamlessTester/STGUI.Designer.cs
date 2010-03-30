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
            this.buttonPropagate = new System.Windows.Forms.Button();
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
            this.labelMin = new System.Windows.Forms.Label();
            this.labelMax = new System.Windows.Forms.Label();
            this.textBoxMinWaitTime = new System.Windows.Forms.TextBox();
            this.textBoxMaxWaitTime = new System.Windows.Forms.TextBox();
            this.labelInstructions = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.buttonClearLogs = new System.Windows.Forms.Button();
            this.labelLogs = new System.Windows.Forms.Label();
            this.buttonCompare = new System.Windows.Forms.Button();
            this.buttonSaveLog = new System.Windows.Forms.Button();
            this.listBoxFilter = new System.Windows.Forms.ListBox();
            this.labelFilter = new System.Windows.Forms.Label();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.buttonFilterAdd = new System.Windows.Forms.Button();
            this.buttonFilterClear = new System.Windows.Forms.Button();
            this.checkBoxBurst = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // textBoxSourcePath
            // 
            this.textBoxSourcePath.Location = new System.Drawing.Point(51, 25);
            this.textBoxSourcePath.Name = "textBoxSourcePath";
            this.textBoxSourcePath.Size = new System.Drawing.Size(135, 20);
            this.textBoxSourcePath.TabIndex = 0;
            // 
            // labelBrowseSource
            // 
            this.labelBrowseSource.AutoSize = true;
            this.labelBrowseSource.Location = new System.Drawing.Point(13, 28);
            this.labelBrowseSource.Name = "labelBrowseSource";
            this.labelBrowseSource.Size = new System.Drawing.Size(32, 13);
            this.labelBrowseSource.TabIndex = 0;
            this.labelBrowseSource.Text = "Path:";
            // 
            // buttonSourceBrowse
            // 
            this.buttonSourceBrowse.Location = new System.Drawing.Point(192, 23);
            this.buttonSourceBrowse.Name = "buttonSourceBrowse";
            this.buttonSourceBrowse.Size = new System.Drawing.Size(50, 23);
            this.buttonSourceBrowse.TabIndex = 1;
            this.buttonSourceBrowse.Text = "Browse";
            this.buttonSourceBrowse.UseVisualStyleBackColor = true;
            this.buttonSourceBrowse.Click += new System.EventHandler(this.buttonSourceBrowse_Click);
            // 
            // buttonSourceAdd
            // 
            this.buttonSourceAdd.Location = new System.Drawing.Point(248, 23);
            this.buttonSourceAdd.Name = "buttonSourceAdd";
            this.buttonSourceAdd.Size = new System.Drawing.Size(50, 23);
            this.buttonSourceAdd.TabIndex = 2;
            this.buttonSourceAdd.Text = "Add";
            this.buttonSourceAdd.UseVisualStyleBackColor = true;
            this.buttonSourceAdd.Click += new System.EventHandler(this.buttonSourceAdd_Click);
            // 
            // listBoxSourcePaths
            // 
            this.listBoxSourcePaths.AllowDrop = true;
            this.listBoxSourcePaths.FormattingEnabled = true;
            this.listBoxSourcePaths.Location = new System.Drawing.Point(16, 51);
            this.listBoxSourcePaths.Name = "listBoxSourcePaths";
            this.listBoxSourcePaths.Size = new System.Drawing.Size(338, 95);
            this.listBoxSourcePaths.TabIndex = 5;
            this.listBoxSourcePaths.DragDrop += new System.Windows.Forms.DragEventHandler(this.listBoxSourcePaths_DragDrop);
            this.listBoxSourcePaths.DragEnter += new System.Windows.Forms.DragEventHandler(this.listBox_DragEnter);
            // 
            // buttonPropagate
            // 
            this.buttonPropagate.Location = new System.Drawing.Point(16, 471);
            this.buttonPropagate.Name = "buttonPropagate";
            this.buttonPropagate.Size = new System.Drawing.Size(100, 23);
            this.buttonPropagate.TabIndex = 5;
            this.buttonPropagate.Text = "Propagate";
            this.buttonPropagate.UseVisualStyleBackColor = true;
            this.buttonPropagate.Click += new System.EventHandler(this.buttonPropagate_Click);
            // 
            // labelDuration
            // 
            this.labelDuration.AutoSize = true;
            this.labelDuration.Location = new System.Drawing.Point(13, 422);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(50, 13);
            this.labelDuration.TabIndex = 6;
            this.labelDuration.Text = "Duration:";
            // 
            // textBoxDuration
            // 
            this.textBoxDuration.Location = new System.Drawing.Point(62, 419);
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
            this.sourcePathsLabel.Size = new System.Drawing.Size(71, 13);
            this.sourcePathsLabel.TabIndex = 3;
            this.sourcePathsLabel.Text = "Repository:";
            // 
            // labelDestPaths
            // 
            this.labelDestPaths.AutoSize = true;
            this.labelDestPaths.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDestPaths.Location = new System.Drawing.Point(12, 149);
            this.labelDestPaths.Name = "labelDestPaths";
            this.labelDestPaths.Size = new System.Drawing.Size(84, 13);
            this.labelDestPaths.TabIndex = 14;
            this.labelDestPaths.Text = "Sync Folders:";
            // 
            // listBoxDestPaths
            // 
            this.listBoxDestPaths.AllowDrop = true;
            this.listBoxDestPaths.FormattingEnabled = true;
            this.listBoxDestPaths.Location = new System.Drawing.Point(16, 191);
            this.listBoxDestPaths.Name = "listBoxDestPaths";
            this.listBoxDestPaths.Size = new System.Drawing.Size(338, 95);
            this.listBoxDestPaths.TabIndex = 13;
            this.listBoxDestPaths.DragDrop += new System.Windows.Forms.DragEventHandler(this.listBoxDestPaths_DragDrop);
            this.listBoxDestPaths.DragEnter += new System.Windows.Forms.DragEventHandler(this.listBox_DragEnter);
            // 
            // buttonDestAdd
            // 
            this.buttonDestAdd.Location = new System.Drawing.Point(248, 162);
            this.buttonDestAdd.Name = "buttonDestAdd";
            this.buttonDestAdd.Size = new System.Drawing.Size(50, 23);
            this.buttonDestAdd.TabIndex = 6;
            this.buttonDestAdd.Text = "Add";
            this.buttonDestAdd.UseVisualStyleBackColor = true;
            this.buttonDestAdd.Click += new System.EventHandler(this.buttonDestAdd_Click);
            // 
            // buttonDestBrowse
            // 
            this.buttonDestBrowse.Location = new System.Drawing.Point(192, 163);
            this.buttonDestBrowse.Name = "buttonDestBrowse";
            this.buttonDestBrowse.Size = new System.Drawing.Size(50, 23);
            this.buttonDestBrowse.TabIndex = 5;
            this.buttonDestBrowse.Text = "Browse";
            this.buttonDestBrowse.UseVisualStyleBackColor = true;
            this.buttonDestBrowse.Click += new System.EventHandler(this.buttonDestBrowse_Click);
            // 
            // labelDestPath
            // 
            this.labelDestPath.AutoSize = true;
            this.labelDestPath.Location = new System.Drawing.Point(13, 168);
            this.labelDestPath.Name = "labelDestPath";
            this.labelDestPath.Size = new System.Drawing.Size(32, 13);
            this.labelDestPath.TabIndex = 10;
            this.labelDestPath.Text = "Path:";
            // 
            // textBoxDest
            // 
            this.textBoxDest.Location = new System.Drawing.Point(51, 165);
            this.textBoxDest.Name = "textBoxDest";
            this.textBoxDest.Size = new System.Drawing.Size(135, 20);
            this.textBoxDest.TabIndex = 4;
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
            this.buttonCancel.Location = new System.Drawing.Point(254, 471);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 23);
            this.buttonCancel.TabIndex = 15;
            this.buttonCancel.Text = "Cancel Test";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(16, 500);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(338, 23);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 16;
            // 
            // buttonClearSource
            // 
            this.buttonClearSource.Location = new System.Drawing.Point(304, 23);
            this.buttonClearSource.Name = "buttonClearSource";
            this.buttonClearSource.Size = new System.Drawing.Size(50, 23);
            this.buttonClearSource.TabIndex = 3;
            this.buttonClearSource.Text = "Clear";
            this.buttonClearSource.UseVisualStyleBackColor = true;
            this.buttonClearSource.Click += new System.EventHandler(this.buttonClearSource_Click);
            // 
            // buttonClearDest
            // 
            this.buttonClearDest.Location = new System.Drawing.Point(304, 162);
            this.buttonClearDest.Name = "buttonClearDest";
            this.buttonClearDest.Size = new System.Drawing.Size(50, 23);
            this.buttonClearDest.TabIndex = 7;
            this.buttonClearDest.Text = "Clear";
            this.buttonClearDest.UseVisualStyleBackColor = true;
            this.buttonClearDest.Click += new System.EventHandler(this.buttonClearDest_Click);
            // 
            // labelMin
            // 
            this.labelMin.AutoSize = true;
            this.labelMin.Location = new System.Drawing.Point(132, 422);
            this.labelMin.Name = "labelMin";
            this.labelMin.Size = new System.Drawing.Size(30, 13);
            this.labelMin.TabIndex = 19;
            this.labelMin.Text = "Min.:";
            // 
            // labelMax
            // 
            this.labelMax.AutoSize = true;
            this.labelMax.Location = new System.Drawing.Point(251, 422);
            this.labelMax.Name = "labelMax";
            this.labelMax.Size = new System.Drawing.Size(33, 13);
            this.labelMax.TabIndex = 20;
            this.labelMax.Text = "Max.:";
            // 
            // textBoxMinWaitTime
            // 
            this.textBoxMinWaitTime.Location = new System.Drawing.Point(181, 419);
            this.textBoxMinWaitTime.Name = "textBoxMinWaitTime";
            this.textBoxMinWaitTime.Size = new System.Drawing.Size(54, 20);
            this.textBoxMinWaitTime.TabIndex = 21;
            this.textBoxMinWaitTime.Text = "1";
            this.textBoxMinWaitTime.TextChanged += new System.EventHandler(this.textBoxMinWaitTime_TextChanged);
            // 
            // textBoxMaxWaitTime
            // 
            this.textBoxMaxWaitTime.Location = new System.Drawing.Point(300, 419);
            this.textBoxMaxWaitTime.Name = "textBoxMaxWaitTime";
            this.textBoxMaxWaitTime.Size = new System.Drawing.Size(54, 20);
            this.textBoxMaxWaitTime.TabIndex = 22;
            this.textBoxMaxWaitTime.Text = "60";
            // 
            // labelInstructions
            // 
            this.labelInstructions.AutoSize = true;
            this.labelInstructions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelInstructions.Location = new System.Drawing.Point(13, 403);
            this.labelInstructions.Name = "labelInstructions";
            this.labelInstructions.Size = new System.Drawing.Size(277, 13);
            this.labelInstructions.TabIndex = 23;
            this.labelInstructions.Text = "Propagation Settings (Please enter in seconds):";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(13, 526);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 24;
            this.labelStatus.Text = "labelStatus";
            this.labelStatus.Visible = false;
            // 
            // textBoxLog
            // 
            this.textBoxLog.BackColor = System.Drawing.Color.Black;
            this.textBoxLog.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxLog.ForeColor = System.Drawing.Color.White;
            this.textBoxLog.Location = new System.Drawing.Point(360, 30);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(414, 464);
            this.textBoxLog.TabIndex = 25;
            // 
            // buttonClearLogs
            // 
            this.buttonClearLogs.Location = new System.Drawing.Point(581, 500);
            this.buttonClearLogs.Name = "buttonClearLogs";
            this.buttonClearLogs.Size = new System.Drawing.Size(193, 23);
            this.buttonClearLogs.TabIndex = 26;
            this.buttonClearLogs.Text = "Clear Logs";
            this.buttonClearLogs.UseVisualStyleBackColor = true;
            this.buttonClearLogs.Click += new System.EventHandler(this.buttonClearLogs_Click);
            // 
            // labelLogs
            // 
            this.labelLogs.AutoSize = true;
            this.labelLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLogs.Location = new System.Drawing.Point(357, 9);
            this.labelLogs.Name = "labelLogs";
            this.labelLogs.Size = new System.Drawing.Size(38, 13);
            this.labelLogs.TabIndex = 27;
            this.labelLogs.Text = "Logs:";
            // 
            // buttonCompare
            // 
            this.buttonCompare.Location = new System.Drawing.Point(135, 471);
            this.buttonCompare.Name = "buttonCompare";
            this.buttonCompare.Size = new System.Drawing.Size(100, 23);
            this.buttonCompare.TabIndex = 28;
            this.buttonCompare.Text = "Verify";
            this.buttonCompare.UseVisualStyleBackColor = true;
            this.buttonCompare.Click += new System.EventHandler(this.buttonCompare_Click);
            // 
            // buttonSaveLog
            // 
            this.buttonSaveLog.Location = new System.Drawing.Point(360, 500);
            this.buttonSaveLog.Name = "buttonSaveLog";
            this.buttonSaveLog.Size = new System.Drawing.Size(193, 23);
            this.buttonSaveLog.TabIndex = 29;
            this.buttonSaveLog.Text = "Save Log";
            this.buttonSaveLog.UseVisualStyleBackColor = true;
            this.buttonSaveLog.Click += new System.EventHandler(this.buttonSaveLog_Click);
            // 
            // listBoxFilter
            // 
            this.listBoxFilter.AllowDrop = true;
            this.listBoxFilter.FormattingEnabled = true;
            this.listBoxFilter.Location = new System.Drawing.Point(15, 318);
            this.listBoxFilter.Name = "listBoxFilter";
            this.listBoxFilter.Size = new System.Drawing.Size(338, 82);
            this.listBoxFilter.TabIndex = 30;
            // 
            // labelFilter
            // 
            this.labelFilter.AutoSize = true;
            this.labelFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFilter.Location = new System.Drawing.Point(13, 295);
            this.labelFilter.Name = "labelFilter";
            this.labelFilter.Size = new System.Drawing.Size(45, 13);
            this.labelFilter.TabIndex = 31;
            this.labelFilter.Text = "Filters:";
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Location = new System.Drawing.Point(62, 292);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(180, 20);
            this.textBoxFilter.TabIndex = 32;
            // 
            // buttonFilterAdd
            // 
            this.buttonFilterAdd.Location = new System.Drawing.Point(248, 290);
            this.buttonFilterAdd.Name = "buttonFilterAdd";
            this.buttonFilterAdd.Size = new System.Drawing.Size(50, 23);
            this.buttonFilterAdd.TabIndex = 33;
            this.buttonFilterAdd.Text = "Add";
            this.buttonFilterAdd.UseVisualStyleBackColor = true;
            this.buttonFilterAdd.Click += new System.EventHandler(this.buttonFilterAdd_Click);
            // 
            // buttonFilterClear
            // 
            this.buttonFilterClear.Location = new System.Drawing.Point(304, 290);
            this.buttonFilterClear.Name = "buttonFilterClear";
            this.buttonFilterClear.Size = new System.Drawing.Size(50, 23);
            this.buttonFilterClear.TabIndex = 34;
            this.buttonFilterClear.Text = "Clear";
            this.buttonFilterClear.UseVisualStyleBackColor = true;
            this.buttonFilterClear.Click += new System.EventHandler(this.buttonFilterClear_Click);
            // 
            // checkBoxBurst
            // 
            this.checkBoxBurst.AutoSize = true;
            this.checkBoxBurst.Location = new System.Drawing.Point(273, 445);
            this.checkBoxBurst.Name = "checkBoxBurst";
            this.checkBoxBurst.Size = new System.Drawing.Size(80, 17);
            this.checkBoxBurst.TabIndex = 35;
            this.checkBoxBurst.Text = "Burst Mode";
            this.checkBoxBurst.UseVisualStyleBackColor = true;
            this.checkBoxBurst.CheckedChanged += new System.EventHandler(this.checkBoxBurst_CheckedChanged);
            // 
            // FormSeamlessTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(784, 547);
            this.Controls.Add(this.checkBoxBurst);
            this.Controls.Add(this.buttonFilterClear);
            this.Controls.Add(this.buttonFilterAdd);
            this.Controls.Add(this.textBoxFilter);
            this.Controls.Add(this.labelFilter);
            this.Controls.Add(this.listBoxFilter);
            this.Controls.Add(this.buttonSaveLog);
            this.Controls.Add(this.buttonCompare);
            this.Controls.Add(this.labelLogs);
            this.Controls.Add(this.buttonClearLogs);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.labelInstructions);
            this.Controls.Add(this.textBoxMaxWaitTime);
            this.Controls.Add(this.textBoxMinWaitTime);
            this.Controls.Add(this.labelMax);
            this.Controls.Add(this.labelMin);
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
            this.Controls.Add(this.buttonPropagate);
            this.Controls.Add(this.listBoxSourcePaths);
            this.Controls.Add(this.buttonSourceAdd);
            this.Controls.Add(this.buttonSourceBrowse);
            this.Controls.Add(this.labelBrowseSource);
            this.Controls.Add(this.textBoxSourcePath);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(800, 585);
            this.MinimumSize = new System.Drawing.Size(800, 585);
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
        private System.Windows.Forms.Button buttonPropagate;
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
        private System.Windows.Forms.Label labelMin;
        private System.Windows.Forms.Label labelMax;
        private System.Windows.Forms.TextBox textBoxMinWaitTime;
        private System.Windows.Forms.TextBox textBoxMaxWaitTime;
        private System.Windows.Forms.Label labelInstructions;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Button buttonClearLogs;
        private System.Windows.Forms.Label labelLogs;
        private System.Windows.Forms.Button buttonCompare;
        private System.Windows.Forms.Button buttonSaveLog;
        private System.Windows.Forms.ListBox listBoxFilter;
        private System.Windows.Forms.Label labelFilter;
        private System.Windows.Forms.TextBox textBoxFilter;
        private System.Windows.Forms.Button buttonFilterAdd;
        private System.Windows.Forms.Button buttonFilterClear;
        private System.Windows.Forms.CheckBox checkBoxBurst;
    }
}

