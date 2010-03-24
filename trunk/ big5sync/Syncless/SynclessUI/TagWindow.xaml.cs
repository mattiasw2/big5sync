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
using Syncless.Core.Exceptions;
using Microsoft.Windows.Controls;
using SynclessUI.Helper;
using Syncless.Core.View;

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
            get { if(ACBName.Text != null) return ACBName.Text.Trim(); else return ""; }
        }
		private bool popupclosed = true;
		
        private string _path;
        
		public TagWindow(MainWindow main, string path, string tagname)
        {
            InitializeComponent();

			_main = main;
			ACBName.IsEnabled = false;

            if (path == "")
            {
                _path = SelectPath(true);
            }
            else
            {
                _path = path;
            }

            ProcessPath(_path, tagname);

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
            string path = (string)Application.Current.Properties["folderlastselected"];
            path = (System.IO.Directory.Exists(path)) ? path : "";
            var browse = new Ionic.Utils.FolderBrowserDialogEx
            {
                Description = "Select the folder to tag",
                ShowNewFolderButton = true,
                ShowEditBox = true,
                NewStyle = true,
                SelectedPath = path,
                ShowFullPathInEditBox = true,
                ShowBothFilesAndFolders = false,
            };

            // Fix Vista Bug
            OperatingSystem os = System.Environment.OSVersion;
            if (os.Version.Major == 6 && os.Version.Minor == 0)
            {
                browse.RootFolder = Environment.SpecialFolder.MyComputer;
            }

            var result = browse.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                path = browse.SelectedPath;
                Application.Current.Properties["folderlastselected"] = path;
                return path;
            }
            else if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                cancelstatus = true;
                _main.Focus();
            }

            return "";
		}

        private void ProcessPath(string path, string _selectedtag)
        {
            try
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
            catch (UnhandledException)
            {
                _main.DisplayUnhandledExceptionMessage();
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
            try
            {
                if (_tagname != "")
                {
                    if (_path != "")
                    {
                        _main.CreateTag(_tagname);

                        bool tocontinue = this.TriggerLongPathWarning();

                        if (tocontinue)
                        {
                            TagView tv1 = null;

                            try
                            {
                                tv1 = _main.gui.Tag(_tagname, new DirectoryInfo(_path));

                                if (tv1 != null)
                                {
                                    _main.InitializeTagList();
                                    _main.SelectTag(_tagname);
                                    this.Close();
                                }
                                else
                                {
                                    DialogsHelper.ShowError("Tag Error", "Tag Error Occured. Please Try Again.");
                                }
                            }
                            catch (Syncless.Tagging.Exceptions.RecursiveDirectoryException)
                            {
                                DialogsHelper.ShowError("Recursive Directory Error", "Folder could not be tagged as it is a sub-folder of a folder already tagged.");
                            }
                            catch (Syncless.Tagging.Exceptions.PathAlreadyExistsException)
                            {
                                DialogsHelper.ShowError("Path Already Exists", "The path you tried to tag is already tagged.");
                            }
                        }
                    }
                    else
                    {
                        DialogsHelper.ShowError("Folder Not Selected", "Please select a folder to tag.");
                    }
                }
                else
                {
                    DialogsHelper.ShowError("Tagname Empty", "Please specify a tagname.");
                }
            }
            catch (UnhandledException)
            {
                _main.DisplayUnhandledExceptionMessage();
            }
		}

        private bool TriggerLongPathWarning()
        {
            if (_path.Length > 200)
            {
                bool result = DialogsHelper.ShowWarning("Long Path Name Warning", "NTFS File System does not handle paths which are 248 characters or more in length properly. \nAre you sure you wish to continue");

                return result;
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
