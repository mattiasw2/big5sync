using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Syncless.Core;
using Microsoft.Windows.Controls;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Visitor;
using System.Data;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for PreviewSyncWindow.xaml
    /// </summary>
    public partial class PreviewSyncWindow : Window
    {		
		private MainWindow _main;
        private DataTable _GameData;
        
		public PreviewSyncWindow(MainWindow main, string _selectedtag)
        {
			this.InitializeDataGrid();
            InitializeComponent();
			
			_main = main;

            RootCompareObject rco = _main.gui.PreviewSync(_selectedtag);

            if (rco != null)
            {

            }
        }
		
		private void InitializeDataGrid() {
			_GameData = new DataTable();
			_GameData.Columns.Add(new DataColumn("Game Name", typeof(string)));
			_GameData.Columns.Add(new DataColumn("Creator", typeof(string)));
			_GameData.Columns.Add(new DataColumn("Publisher", typeof(string)));
			_GameData.Columns.Add(new DataColumn("On Xbox", typeof(bool)));
		
			var row = _GameData.NewRow();
			_GameData.Rows.Add(row);
			row["Game Name"] = "World Of Warcraft";
			row["Creator"] = "Blizzard";
			row["Publisher"] = "Blizzard";
			row["On Xbox"] = false;
		
			row = _GameData.NewRow();
			_GameData.Rows.Add(row);
			row["Game Name"] = "Halo 3";
			row["Creator"] = "Bungie";
			row["Publisher"] = "Microsoft";
			row["On Xbox"] = true;
		
			row = _GameData.NewRow();
			_GameData.Rows.Add(row);
			row["Game Name"] = "Gears Of War";
			row["Creator"] = "Epic";
			row["Publisher"] = "Microsoft";
			row["On Xbox"] = true;
		
			InitializeComponent();
		
			_PublisherCombo.ItemsSource = new List<string>() { "Activision", "Ubisoft",
				"Microsoft", "Blizzard", "Nintendo", "Electronic Arts", "Take-Two Interactive" };
		}

		public DataTable GameData
		{ get { return _GameData; } }
		
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			this.Close();
        }
		
		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
