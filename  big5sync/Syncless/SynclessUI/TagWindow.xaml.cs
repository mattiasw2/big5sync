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
    /// Interaction logic for TagWindow.xaml
    /// </summary>
    public partial class TagWindow : Window
    {		
		private MainWindow _main;
        private string _selectedtype;
		private bool cancelstatus = false;
        private string _tagname {
            get { return ACBTagName.GetText.Trim(); }
        }
		
        private string _path;
        
		public TagWindow(MainWindow main)
        {
            InitializeComponent();
			
			_main = main;
            _selectedtype = "";
			ACBTagName.IsEnabled = false;
            
            _path = SelectFileFolder(true);
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
					ACBTagName.IsEnabled = true;
                    ACBTagName.MySourceList = _main.gui.GetAllFileTags();
                }
                else if (di.Exists)
                {
					var uriSource = new Uri(@"/SynclessUI;component/Icons/folder.ico", UriKind.Relative);
					TagIcon.Source = new BitmapImage(uriSource);
					
                    _selectedtype = "Folder";
                    TxtBoxPath.Text = path;
					ACBTagName.IsEnabled = true;
                    ACBTagName.MySourceList = _main.gui.GetAllFolderTags();
                }
                else
                {
                    ACBTagName.IsEnabled = false;
                    ACBTagName.MySourceList = new List<string>();
                }
            }
        }
		
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if(_tagname != "") {
				if(_selectedtype != "") {
					bool proceedtotag = false;
					bool compatibletagtype = false;
					
					TagView tv = _main.gui.GetTag(_tagname);
					
					if(tv == null) {
						if(_selectedtype == "File")
						{
							proceedtotag = _main.CreateFileTag(_tagname);
							compatibletagtype = true;
						} else if(_selectedtype == "Folder")
						{
							proceedtotag = _main.CreateFolderTag(_tagname);
							compatibletagtype = true;
						}
					} else {
						if(_selectedtype == "Folder") {
							if(tv is FolderTagView) {
								compatibletagtype = true;
								proceedtotag = true;
							}
						} else if(_selectedtype == "File") {
							if(tv is FileTagView) {
								compatibletagtype = true;
								proceedtotag = true;
							}
						} 
					}
					
					if(proceedtotag && compatibletagtype) {
						TagView tv1 = null;
						
						if(_selectedtype == "File") {
							tv1 = _main.gui.TagFile(_tagname, new FileInfo(_path));
						} else if(_selectedtype == "Folder") {
							tv1 = _main.gui.TagFolder(_tagname, new DirectoryInfo(_path));
						}
						
						if(tv1 != null) {
							_main.InitializeTagList();
							_main.SelectTag(_tagname);
						} else {
							string messageBoxText = "Tag Error Occcured. Please Try Again.";
							string caption = "Tag Error";
							MessageBoxButton button = MessageBoxButton.OK;
							MessageBoxImage icon = MessageBoxImage.Error;
			
							MessageBox.Show(messageBoxText, caption, button, icon);
						}
						this.Close();
					}
					
					if(!compatibletagtype) {
						string messageBoxText = "Please select an approriate tag type";
						string caption = "Tag Type Incompatible";
						MessageBoxButton button = MessageBoxButton.OK;
						MessageBoxImage icon = MessageBoxImage.Error;
		
						MessageBox.Show(messageBoxText, caption, button, icon);
					} else if(!proceedtotag) {
						string messageBoxText = "File/Folder Tag Error";
						string caption = "File/Folder Not Tagged";
						MessageBoxButton button = MessageBoxButton.OK;
						MessageBoxImage icon = MessageBoxImage.Error;
		
						MessageBox.Show(messageBoxText, caption, button, icon);
					}
				} else {
					string messageBoxText = "Please select a file/folder to tag.";
					string caption = "File/Folder Not Selected";
					MessageBoxButton button = MessageBoxButton.OK;
					MessageBoxImage icon = MessageBoxImage.Error;
	
					MessageBox.Show(messageBoxText, caption, button, icon);
				}
			} else {
                string messageBoxText = "Please specify a tagname.";
                string caption = "Tagname Empty";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);
			}
        }
		
		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

		private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Keyboard.Focus(ACBTagName);
		}

		private void BtnBrowse_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            string path = SelectFileFolder(false);
            ProcessPath(path);
		}
    }
}
