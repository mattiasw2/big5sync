using System;
using System.Windows;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Visitor;
using System.Data;
using SynclessUI.Visitor;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for PreviewSyncWindow.xaml
    /// </summary>
    public partial class PreviewSyncWindow : Window
    {
        private readonly MainWindow _main;
        private DataTable _previewSyncData;

        public PreviewSyncWindow(MainWindow main, string selectedTag)
        {

            _previewSyncData = new DataTable();
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.Source, typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.Operation, typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn(PreviewVisitor.Dest, typeof(string)));
            _main = main;



            Populate(_main.Gui.PreviewSync(selectedTag));
            InitializeDataGrid();
            //PreviewSyncDelegate previewDelegate = new PreviewSyncDelegate(_main.Gui.PreviewSync);
            //previewDelegate.BeginInvoke(selectedTag, CallBack, previewDelegate);

        }
        private delegate RootCompareObject PreviewSyncDelegate(string tagname);
        private void CallBack(IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                PreviewSyncDelegate previewDel = result.AsyncState as PreviewSyncDelegate;
                RootCompareObject rco = previewDel.EndInvoke(result);

                Populate(rco);
                
            }
            else
            {
                Console.WriteLine("error");
            }
            
        }
        private void Populate(RootCompareObject rco)
        {
            PreviewVisitor visitor = new PreviewVisitor(_previewSyncData);
            if (rco != null)
            {

                SyncUIHelper.TraverseFolderHelper(rco, visitor);
            }

        }

        private delegate void UpdateDelegate();
        private void InitializeDataGrid()
        {

            InitializeComponent();
        }

        public DataTable PreviewSyncData
        { get { return _previewSyncData; } }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void Canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseWindow()
        {
            FormFadeOut.Begin();
        }

        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
