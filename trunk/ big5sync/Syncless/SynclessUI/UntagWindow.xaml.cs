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
using Ionic.Utils;
using System.IO;
using Syncless.Core;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for UntagWindow.xaml
    /// </summary>
    public partial class UntagWindow : Window
    {		
		private MainWindow _main;
        private string _selectedtype;
		private bool cancelstatus = false;
		
        private string _path;
        
		public UntagWindow(MainWindow main, string clipath)
        {
            InitializeComponent();
			
			_main = main;
            _selectedtype = "";

            if (clipath == "")
            {
                _path = SelectFileFolder(true);
            }
            else
            {
                _path = clipath;
            }
            ProcessPath(_path);

            if (cancelstatus)
            {
                this.Close();
            }
            else
            {
                this.ShowDialog();
            }
        }
		
		private string SelectFileFolder(bool cancelStatus) {
			string path = "";
			// _folderName = (System.IO.Directory.Exists(_folderName)) ? _folderName : "";
			var dlg1 = new Ionic.Utils.FolderBrowserDialogEx
			{
				Description = "Select the file/folder to tag",
				ShowNewFolderButton = true,
				ShowEditBox = true,
			    NewStyle = true,
				SelectedPath = path,
				ShowFullPathInEditBox= true,
			};
			dlg1.RootFolder = System.Environment.SpecialFolder.MyComputer;
		
			var result = dlg1.ShowDialog();
		
		
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				path = dlg1.SelectedPath;
                return path;
			} else if(result == System.Windows.Forms.DialogResult.Cancel)
			{
				cancelstatus = true;
			}
	
			return "";
		}

        private void ProcessPath(string path)
        {
            if (path == "")
            {
                _selectedtype = "";
            }
            else
            {
                FileInfo fi = new FileInfo(path);
                DirectoryInfo di = new DirectoryInfo(path);
                if (fi.Exists)
                {
					var uriSource = new Uri(@"/SynclessUI;component/Icons/file.ico", UriKind.Relative);
					TagIcon.Source = new BitmapImage(uriSource);
					
                    _selectedtype = "File";
                    TxtBoxPath.Text = path;
                }
                else if (di.Exists)
                {
					var uriSource = new Uri(@"/SynclessUI;component/Icons/folder.ico", UriKind.Relative);
					TagIcon.Source = new BitmapImage(uriSource);
					
                    _selectedtype = "Folder";
                    TxtBoxPath.Text = path;
                }
                else
                {
                }
            }
        }
		
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        }
		
		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

		private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
		}
    }
}
