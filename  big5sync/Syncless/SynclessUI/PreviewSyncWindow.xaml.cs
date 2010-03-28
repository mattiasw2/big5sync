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
			this.InitializeDataGrid();
            InitializeComponent();
			
			_main = main;

            RootCompareObject rco = _main.Gui.PreviewSync(selectedTag);

            if (rco != null)
            {
                // SyncUIHelper.TraverseFolderHelper(rco, new SyncerVisitor(request.Config, _previewSyncData));
            }
        }
		
		private void InitializeDataGrid() {
            _previewSyncData = new DataTable();
            _previewSyncData.Columns.Add(new DataColumn("Path1", typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn("Operation", typeof(string)));
            _previewSyncData.Columns.Add(new DataColumn("Path2", typeof(string)));

            var row = _previewSyncData.NewRow();
            _previewSyncData.Rows.Add(row);
			row["Path1"] = "World Of Warcraft";
			row["Operation"] = "Blizzard";
			row["Path2"] = "Blizzard";

            row = _previewSyncData.NewRow();
            _previewSyncData.Rows.Add(row);
			row["Path1"] = "Halo 3";
			row["Operation"] = "Bungie";
			row["Path2"] = "Microsoft";

            row = _previewSyncData.NewRow();
            _previewSyncData.Rows.Add(row);
			row["Path1"] = "Gears Of War";
			row["Operation"] = "Epic";
			row["Path2"] = "Microsoft";
		
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
