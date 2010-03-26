using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SynclessSeamlessTester
{
    public partial class FormSeamlessTester : Form
    {
        private List<string> _sourcePaths, _destPaths;

        public FormSeamlessTester()
        {
            InitializeComponent();
            Init();
        }


        private void Init()
        {
            _sourcePaths = new List<string>();
            _destPaths = new List<string>();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxSourcePath.Text = folderBrowserDialog1.SelectedPath;
                folderBrowserDialog1.Reset();
            }
        }

        private void buttonDestBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxDest.Text = folderBrowserDialog1.SelectedPath;
                folderBrowserDialog1.Reset();
            }
        }

        private void buttonSourceAdd_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBoxSourcePath.Text) && !_destPaths.Contains(textBoxSourcePath.Text))
            {
                listBoxSourcePaths.Items.Clear();
                listBoxSourcePaths.Items.AddRange(AddToSource(textBoxSourcePath.Text).ToArray());
            }
            textBoxSourcePath.Text = string.Empty;
        }

        private void buttonDestAdd_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBoxDest.Text) && !_sourcePaths.Contains(textBoxDest.Text))
            {
                listBoxDestPaths.Items.Clear();
                listBoxDestPaths.Items.AddRange(AddToDest(textBoxDest.Text).ToArray());
            }
            textBoxDest.Text = string.Empty;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBoxSourcePaths.Items.Count > 0 && listBoxDestPaths.Items.Count > 0 &&
                    Convert.ToInt32(textBoxDuration.Text) > 0 && Convert.ToInt32(textBoxMinWaitTime.Text) >= 0 && Convert.ToInt32(textBoxMaxWaitTime.Text) >= Convert.ToInt32(textBoxMinWaitTime.Text))
                {
                    button1.Enabled = false;
                    buttonCancel.Enabled = true;
                    ToggleAllControls(false);
                    backgroundWorker1.RunWorkerAsync(new TestInfo());
                    this.Cursor = Cursors.WaitCursor;
                }

            }
            catch (FormatException) { }
            catch (OverflowException) { }
        }

        private void listBoxSourcePaths_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var foldernames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                if (foldernames != null)
                    foreach (string i in foldernames)
                    {
                        if (_destPaths.Contains(i))
                            continue;
                        var folder = new DirectoryInfo(i);
                        if (folder.Exists)
                        {
                            listBoxSourcePaths.Items.Clear();
                            listBoxSourcePaths.Items.AddRange(AddToSource(i).ToArray());
                        }
                    }
            }
        }

        private void listBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void listBoxDestPaths_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var foldernames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                if (foldernames != null)
                    foreach (string i in foldernames)
                    {
                        if (_sourcePaths.Contains(i))
                            continue;
                        var folder = new DirectoryInfo(i);
                        if (folder.Exists)
                        {
                            listBoxDestPaths.Items.Clear();
                            listBoxDestPaths.Items.AddRange(AddToDest(i).ToArray());
                        }
                    }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            TestInfo info = e.Argument as TestInfo;
            new TestWorkerClass(Convert.ToInt32(textBoxDuration.Text), Convert.ToInt32(textBoxMinWaitTime.Text), Convert.ToInt32(textBoxMaxWaitTime.Text), _sourcePaths, _destPaths,
                                info, backgroundWorker1);
            info.Propagated = true;
            new VerifyWorkerClass(_destPaths, info, backgroundWorker1);
            e.Result = info;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            buttonCancel.Enabled = false;
            progressBar1.Value = 100;
            ToggleAllControls(true);
            this.Cursor = Cursors.Arrow;
            TestInfo info = e.Result as TestInfo;
            Console.WriteLine("RESULT: " + (info.Passed ? "PASSED" : "FAILED"));
        }

        public List<string> AddToSource(string s)
        {
            _sourcePaths.Add(s);
            return _sourcePaths;
        }

        public List<string> AddToDest(string s)
        {
            _destPaths.Add(s);
            return _destPaths;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            buttonCancel.Enabled = false;
            ToggleAllControls(true);
            backgroundWorker1.CancelAsync();
            progressBar1.Value = 0;
            this.Cursor = Cursors.Arrow;
        }

        private void ToggleAllControls(bool enable)
        {
            textBoxSourcePath.Enabled = enable;
            listBoxSourcePaths.Enabled = enable;
            buttonSourceBrowse.Enabled = enable;
            buttonSourceAdd.Enabled = enable;

            textBoxDest.Enabled = enable;
            listBoxDestPaths.Enabled = enable;
            buttonDestBrowse.Enabled = enable;
            buttonDestAdd.Enabled = enable;

            textBoxDuration.Enabled = enable;
            textBoxMaxWaitTime.Enabled = enable;
            textBoxMinWaitTime.Enabled = enable;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void buttonClearSource_Click(object sender, EventArgs e)
        {
            _sourcePaths.Clear();
            listBoxSourcePaths.Items.Clear();
        }

        private void buttonClearDest_Click(object sender, EventArgs e)
        {
            _destPaths.Clear();
            listBoxDestPaths.Items.Clear();
        }

        private void textBoxMinWaitTime_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToInt32(textBoxMinWaitTime.Text) < 1)
                    textBoxMinWaitTime.Text = "1";
            }
            catch (Exception)
            {
                textBoxMinWaitTime.Text = "1";
            }
        }

    }
}
