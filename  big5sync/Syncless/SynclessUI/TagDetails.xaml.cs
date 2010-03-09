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
    /// Interaction logic for TagDetails.xaml
    /// </summary>
    public partial class TagDetails : Window
    {		
		private MainWindow _main;
		private bool cancelstatus = false;
        private string _tagname {
            get { return ACBTagName.GetText.Trim(); }
        }
		
        private string _path;
        
		public TagDetails(MainWindow main, string clipath)
        {
            InitializeComponent();
			
			_main = main;
			ACBTagName.IsEnabled = false;

            if (clipath == "")
            {
                _path = SelectPath(true);
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
		
		private string SelectPath(bool cancelStatus) {
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
            if (path != "")
            {
                DirectoryInfo di = new DirectoryInfo(path);
				if (di.Exists)
                {
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
				if(_path != "") {
					bool proceedtotag = false;
					
					TagView tv = _main.gui.GetTag(_tagname);
					
					if(tv == null) {
                        bool result = _main.CreateTag(_tagname);
                        proceedtotag = !result;
					} else {
						proceedtotag = true;
					}
					
					if(proceedtotag) {
						TagView tv1 = _main.gui.TagFolder(_tagname, new DirectoryInfo(_path));
						
						if(tv1 != null) {
							_main.InitializeTagList();
							_main.SelectTag(_tagname);
						} else {
							string messageBoxText = "Tag Error Occured. Please Try Again.";
							string caption = "Tag Error";
							MessageBoxButton button = MessageBoxButton.OK;
							MessageBoxImage icon = MessageBoxImage.Error;
			
							MessageBox.Show(messageBoxText, caption, button, icon);
						}
						this.Close();
					}
					
					if(!proceedtotag) {
						string messageBoxText = "Folder Tag Error";
						string caption = "Folder Not Tagged";
						MessageBoxButton button = MessageBoxButton.OK;
						MessageBoxImage icon = MessageBoxImage.Error;
		
						MessageBox.Show(messageBoxText, caption, button, icon);
                        this.Close();
					}
				} else {
					string messageBoxText = "Please select a folder to tag.";
					string caption = "Folder Not Selected";
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
            string path = SelectPath(false);
            ProcessPath(path);
		}
    }
}
