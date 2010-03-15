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
using Microsoft.Windows.Controls;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for TagWindow.xaml
    /// </summary>
    public partial class TagWindow : Window
    {		
		private MainWindow _main;
		private bool cancelstatus = false;
        private string _tagname {
            get { return ACBName.Text.Trim(); }
        }
		private bool popupclosed = true;
		
        private string _path;
        
		public TagWindow(MainWindow main, string clipath, string _selectedtag)
        {
            InitializeComponent();

			_main = main;
			ACBName.IsEnabled = false;

            if (clipath == "")
            {
                _path = SelectPath(true);
            }
            else
            {
                _path = clipath;
            }

            ProcessPath(_path, _selectedtag);

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
            string path = (string) Application.Current.Properties["folderlastselected"];
            path = (System.IO.Directory.Exists(path)) ? path : "";
			var dlg1 = new Ionic.Utils.FolderBrowserDialogEx
			{
				Description = "Select the folder to tag",
				ShowNewFolderButton = true,
				ShowEditBox = true,
			    NewStyle = true,
                SelectedPath = path,
				ShowFullPathInEditBox= true,
                ShowBothFilesAndFolders = false,
			};
		
			var result = dlg1.ShowDialog();
		
			if (result == System.Windows.Forms.DialogResult.OK)
			{
                path = dlg1.SelectedPath;
                Application.Current.Properties["folderlastselected"] = path;
                return path;
			} else if(result == System.Windows.Forms.DialogResult.Cancel)
			{
				cancelstatus = true;
			}
	
			return "";
		}

        private void ProcessPath(string path, string _selectedtag)
        {
            if (path != "")
            {
                DirectoryInfo di = new DirectoryInfo(path);
				if (di.Exists)
                {
                    TxtBoxPath.Text = path;
                    ACBName.IsEnabled = true;
                    ACBName.ItemsSource = _main.gui.GetAllTags();
                    ACBName.Text = _selectedtag;
                }
                else
                {
                    ACBName.IsEnabled = false;
                    ACBName.ItemsSource = new List<string>();
                }
            }
        }
		
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			ProcessTagging();
        }
		
		private void ProcessTagging() {
			
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

                    bool tocontinue = this.TriggerLongPathWarning();

                    if (tocontinue)
                    {
                        if (proceedtotag)
                        {
							TagView tv1 = null;
							
							try {
                            	tv1 = _main.gui.Tag(_tagname, new DirectoryInfo(_path));
								
								if (tv1 != null)
								{
									_main.InitializeTagList();
									_main.SelectTag(_tagname);
                            		this.Close();
								}
								else
								{
									string messageBoxText = "Tag Error Occured. Please Try Again.";
									string caption = "Tag Error";
									MessageBoxButton button = MessageBoxButton.OK;
									MessageBoxImage icon = MessageBoxImage.Error;
	
									MessageBox.Show(messageBoxText, caption, button, icon);
								}
							} catch(Syncless.Tagging.Exceptions.RecursiveDirectoryException) {
								string messageBoxText = "Folder could not be tagged as it is a sub-folder of a folder already tagged.";
								string caption = "Recursive Directory Error";
								MessageBoxButton button = MessageBoxButton.OK;
								MessageBoxImage icon = MessageBoxImage.Error;
	
								MessageBox.Show(messageBoxText, caption, button, icon);
							} catch(Syncless.Tagging.Exceptions.PathAlreadyExistsException) {
								string messageBoxText = "The path you tried to tag is already tagged.";
								string caption = "Path Already Exists";
								MessageBoxButton button = MessageBoxButton.OK;
								MessageBoxImage icon = MessageBoxImage.Error;
	
								MessageBox.Show(messageBoxText, caption, button, icon);
							}
                        }
                        else
                        {
                            string messageBoxText = "Folder Tag Error";
                            string caption = "Folder Not Tagged";
                            MessageBoxButton button = MessageBoxButton.OK;
                            MessageBoxImage icon = MessageBoxImage.Error;

                            MessageBox.Show(messageBoxText, caption, button, icon);
                            this.Close();
                        }
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

        private bool TriggerLongPathWarning()
        {
            if (_path.Length > 200)
            {
                string messageBoxText = "NTFS File System does not handle paths which are 248 characters or more in length properly. \nAre you sure you wish to continue";
                string caption = "Long Path Name Warning";
                MessageBoxButton button = MessageBoxButton.OKCancel;
                MessageBoxImage icon = MessageBoxImage.Warning;

                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

                switch (result)
                {
                    case MessageBoxResult.OK:
                        return true;
                    case MessageBoxResult.No:
                        return false;
                }
            }

            return true;

        }
		
		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

		private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Keyboard.Focus(ACBName);
		}

		private void BtnBrowse_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            string path = SelectPath(false);
            ProcessPath(path, "");
		}

		/// <summary>
		///	Ugly Hack for Overwritting the ACB Enter Behavior because it does not accept the Enter Key
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Text_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if(e.Key == Key.Enter && popupclosed == true) {
				BtnOk.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
			}
		}

		private void Popup_Opened(object sender, System.EventArgs e)
		{
			popupclosed = false;
		}

		private void Popup_Closed(object sender, System.EventArgs e)
		{
			popupclosed = true;
		}
    }
}
