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
        private TestInfo _testInfo;

        private enum Action
        {
            Propagate,
            Verify,
            Cancel,
            Complete
        }

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

        private void buttonSourceBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxSourcePath.Text = folderBrowserDialog1.SelectedPath;
                folderBrowserDialog1.Reset();
            }
        }

        private void buttonSourceAdd_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBoxSourcePath.Text) && !_sourcePaths.Contains(textBoxDest.Text) && !_destPaths.Contains(textBoxSourcePath.Text))
            {
                listBoxSourcePaths.Items.Clear();
                listBoxSourcePaths.Items.AddRange(AddToSource(textBoxSourcePath.Text).ToArray());
            }
            textBoxSourcePath.Text = string.Empty;
        }

        private void listBoxSourcePaths_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var foldernames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                if (foldernames != null)
                    foreach (string i in foldernames)
                    {
                        if (_sourcePaths.Contains(i) || _destPaths.Contains(i))
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

        private void buttonClearSource_Click(object sender, EventArgs e)
        {
            _sourcePaths.Clear();
            listBoxSourcePaths.Items.Clear();
        }

        private void buttonDestBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxDest.Text = folderBrowserDialog1.SelectedPath;
                folderBrowserDialog1.Reset();
            }
        }

        private void buttonDestAdd_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(textBoxDest.Text) && !_sourcePaths.Contains(textBoxDest.Text) && !_destPaths.Contains(textBoxSourcePath.Text))
            {
                listBoxDestPaths.Items.Clear();
                listBoxDestPaths.Items.AddRange(AddToDest(textBoxDest.Text).ToArray());
            }
            textBoxDest.Text = string.Empty;
        }

        private void buttonClearDest_Click(object sender, EventArgs e)
        {
            _destPaths.Clear();
            listBoxDestPaths.Items.Clear();
        }

        private void listBoxDestPaths_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var foldernames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                if (foldernames != null)
                    foreach (string i in foldernames)
                    {
                        if (_sourcePaths.Contains(i) || _destPaths.Contains(i))
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

        private void listBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void buttonPropagate_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBoxSourcePaths.Items.Count > 0 && listBoxDestPaths.Items.Count > 0 &&
                    Convert.ToInt32(textBoxDuration.Text) > 0 && Convert.ToInt32(textBoxMinWaitTime.Text) >= 0 && Convert.ToInt32(textBoxMaxWaitTime.Text) >= Convert.ToInt32(textBoxMinWaitTime.Text))
                {
                    ToggleAllControls(Action.Propagate);
                    _testInfo = new TestInfo();
                    backgroundWorker1.RunWorkerAsync(_testInfo);
                }

            }
            catch (FormatException) { }
            catch (OverflowException) { }
        }

        private void buttonCompare_Click(object sender, EventArgs e)
        {
            if (_testInfo == null)
            {
                _testInfo = new TestInfo();
                _testInfo.Propagated = true;
            }

            backgroundWorker1.RunWorkerAsync(_testInfo);
            Cursor = Cursors.WaitCursor;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
            ToggleAllControls(Action.Cancel);
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

        public List<string> AddToSource(string s)
        {
            _sourcePaths.Add(s);
            return _sourcePaths;
        }

        public List<string> AddToDest(string s)
        {
            _testInfo = null;
            _destPaths.Add(s);
            return _destPaths;
        }

        private void ToggleAllControls(Action action)
        {
            bool enable = true;

            switch (action)
            {
                case Action.Cancel:
                case Action.Complete:
                    buttonPropagate.Enabled = true;
                    buttonCompare.Enabled = true;
                    buttonCancel.Enabled = false;
                    Cursor = Cursors.Arrow;
                    break;
                case Action.Propagate:
                case Action.Verify:
                    buttonPropagate.Enabled = false;
                    buttonCompare.Enabled = false;
                    buttonCancel.Enabled = true;
                    enable = false;
                    Cursor = Cursors.WaitCursor;
                    break;
            }

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

        #region BackgroundWorker Events

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            TestInfo info = e.Argument as TestInfo;
            if (!info.Propagated)
            {
                StartPropagating();
                new TestWorkerClass(Convert.ToInt32(textBoxDuration.Text), Convert.ToInt32(textBoxMinWaitTime.Text),
                                    Convert.ToInt32(textBoxMaxWaitTime.Text), _sourcePaths, _destPaths,
                                    info, backgroundWorker1, e);
                info.Propagated = true;
            }
            if (info.Propagated)
            {
                StartVerifying();
                new VerifyWorkerClass(_destPaths, info, backgroundWorker1, e);
                e.Result = info;
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                CancelTest();
            else
            {
                CompleteTest();
                ToggleAllControls(Action.Complete);
                TestInfo info = e.Result as TestInfo;
                if (info.Passed.HasValue)
                {
                    ProcessVerifierResults(info);
                    Console.WriteLine("RESULT: " + ((bool)info.Passed ? "PASSED" : "FAILED"));
                }
            }
        }

        #endregion

        private void ProcessVerifierResults(TestInfo info)
        {
            AppendLog("----- START OF VERIFICATION -----");
            AppendLog(DateTime.Now.ToString());
            if (info.Passed.HasValue)
                AppendLog("RESULTS: " + ((bool)info.Passed ? "PASSED." : "FAILED"));

            if (!(bool)info.Passed)
            {
                foreach (LazyFileCompare file in info.FileResults)
                    AppendLog(file.ToString());

                foreach (LazyFolderCompare folder in info.FolderResults)
                    AppendLog(folder.ToString());
            }
            AppendLog("----- END OF VERIFICATION -----");
            AppendLog(string.Empty);
        }

        private void AppendLog(string msg)
        {
            textBoxLog.AppendText(string.Format("{0}\r\n", msg));
        }

        #region Progress Changes

        private void StartPropagating()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(StartPropagating));
            }
            else
            {
                progressBar1.Style = ProgressBarStyle.Continuous;
                labelStatus.Visible = true;
                labelStatus.Text = "Randomly propagating stuff...";
            }
        }

        private void StartVerifying()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(StartVerifying));
            }
            else
            {
                progressBar1.Style = ProgressBarStyle.Marquee;
                labelStatus.Visible = true;
                labelStatus.Text = "Verifying folder contents...";
            }
        }

        private void CompleteTest()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(CompleteTest));
            }
            else
            {
                progressBar1.Style = ProgressBarStyle.Continuous;
                labelStatus.Visible = true;
                labelStatus.Text = "Last action completed at " + DateTime.Now;
                progressBar1.Value = 100;
            }
        }

        private void CancelTest()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(CancelTest));
            }
            else
            {
                progressBar1.Style = ProgressBarStyle.Continuous;
                labelStatus.Visible = true;
                labelStatus.Text = "Last action cancelled at " + DateTime.Now;
                progressBar1.Value = 0;
            }
        }

        #endregion

        private void buttonClearLogs_Click(object sender, EventArgs e)
        {
            textBoxLog.Clear();
        }

    }
}
