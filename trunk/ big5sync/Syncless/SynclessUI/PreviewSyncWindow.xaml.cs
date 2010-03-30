using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Syncless.CompareAndSync.CompareObject;
using SynclessUI.Visitor;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for PreviewSyncWindow.xaml
    /// </summary>
    public partial class PreviewSyncWindow : Window
    {
        private readonly MainWindow _main;
        private readonly DataTable _previewSyncData;
        private readonly string _selectedTag;
        private BackgroundWorker previewWorker;
        private RootCompareObject rco;

        public PreviewSyncWindow(MainWindow main, string selectedTag)
        {
            _selectedTag = selectedTag;

            _previewSyncData = new DataTable();
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.Source, typeof (string)));
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.Operation, typeof (string)));
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.Dest, typeof (string)));
            _main = main;

            Populate(_main.Gui.PreviewSync(selectedTag));
            InitializeDataGrid();

            //PreviewSyncDelegate previewDelegate = new PreviewSyncDelegate(_main.Gui.PreviewSync);
            //previewDelegate.BeginInvoke(selectedTag, CallBack, previewDelegate);
            //previewWorker = new BackgroundWorker();
            //this.previewWorker.DoWork += new DoWorkEventHandler(GetRCO);
            //this.previewWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            //this.previewWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
        }

        public DataTable PreviewSyncData
        {
            get { return _previewSyncData; }
        }

        private void GetRCO(object sender, DoWorkEventArgs e)
        {
            rco = _main.Gui.PreviewSync(_selectedTag);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Populate(rco);
        }

        private void CallBack(IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                var previewDel = result.AsyncState as PreviewSyncDelegate;
                RootCompareObject rco = previewDel.EndInvoke(result);

                Populate(rco);
                Console.WriteLine("Populated");
            }
            else
            {
                Console.WriteLine("error");
            }
        }

        private void Populate(RootCompareObject rco)
        {
            _previewSyncData.Rows.Clear();

            var visitor = new PreviewVisitor(_previewSyncData);
            if (rco != null)
            {
                SyncUIHelper.TraverseFolderHelper(rco, visitor);
            }
            //new UpdateDelegate(Data.InvalidateVisual).BeginInvoke(Test, null);
        }

        public void Test(IAsyncResult result)
        {
            Console.WriteLine("Call back Hit");
            Console.WriteLine(result.CompletedSynchronously);
            Console.WriteLine(result.IsCompleted);
            Console.WriteLine(result.AsyncWaitHandle);
        }

        private void InitializeDataGrid()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseWindow()
        {
            FormFadeOut.Begin();
        }

        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));

            e.Handled = true;
        }

        #region Nested type: PreviewSyncDelegate

        private delegate RootCompareObject PreviewSyncDelegate(string tagname);

        #endregion

        #region Nested type: UpdateDelegate

        private delegate void UpdateDelegate();

        #endregion
    }
}