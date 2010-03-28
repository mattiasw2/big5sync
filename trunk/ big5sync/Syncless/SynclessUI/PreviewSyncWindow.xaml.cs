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
            
			
			_main = main;
            _previewSyncData = new DataTable();
            _previewSyncData.Columns.Add(new DataColumn("Path1", typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn("Operation", typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn("Path2", typeof(string)));

            RootCompareObject rco = _main.Gui.PreviewSync(selectedTag);
            PreviewVisitor visitor = new PreviewVisitor(_previewSyncData);
            if (rco != null)
            {

                SyncUIHelper.TraverseFolderHelper(rco, visitor);
            }
            _previewSyncData = visitor.SyncData;
            this.InitializeDataGrid();

        }
		
		private void InitializeDataGrid() {
		
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
		
		private void CloseWindow() {
            FormFadeOut.Begin();
		}
		
        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
