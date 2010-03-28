using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using Syncless.Core.Exceptions;
using Syncless.Core.View;
using SynclessUI.Helper;
using Button = System.Windows.Controls.Button;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for TagWindow.xaml
    /// </summary>
    public partial class TagWindow : Window
    {		
		private readonly MainWindow _main;
		private bool _cancelstatus = false;
        private string Tagname {
            get { if(ACBName.Text != null) return ACBName.Text.Trim(); else return ""; }
        }
		private bool _popupclosed = true;
		
        private string _path;
        private string _selectedTag;
        private VistaFolderBrowserDialog vistafolderDialog = new VistaFolderBrowserDialog();
        private FolderBrowserDialog _ofd = new FolderBrowserDialog();
        private const string DialogDescription = "Please select a folder to tag.";
        
		public TagWindow(MainWindow main, string path, string tagname)
        {
            InitializeComponent();

			_main = main;
            _selectedTag = tagname;
            
            InitializeFolderDialogs();

		    ACBName.IsEnabled = false;

            _path = path == "" ? SelectPath() : path;

		    if (_cancelstatus)
		    {
		        this.Close();
		    }
		    else
		    {
                ProcessPath(_path, _selectedTag);
		        this.ShowDialog();
		    }
        }

        private void InitializeFolderDialogs()
        {
            vistafolderDialog.Description = DialogDescription;
            vistafolderDialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
            vistafolderDialog.ShowNewFolderButton = true;
            _ofd.Description = DialogDescription;
            _ofd.ShowNewFolderButton = true;
        }

        private string SelectPath() {
		    string path = "";

            if(VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
            {
                if ((bool)vistafolderDialog.ShowDialog())
                {
                    path = vistafolderDialog.SelectedPath;
                } else
                {
                    _cancelstatus = true;
                }

            } else
            {
                DialogResult result = _ofd.ShowDialog();

                if(result == System.Windows.Forms.DialogResult.OK)
                {
                    path = _ofd.SelectedPath;
                }
                else
                {
                    _cancelstatus = true;
                }
            }

		    return path;
		}

        private void ProcessPath(string path, string selectedTag)
        {
            try
            {
                if (path != "")
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    if (di.Exists && !FileHelper.IsZipFile(path))
                    {
                        TxtBoxPath.Text = path;
                        ACBName.IsEnabled = true;
                        ACBName.ItemsSource = _main.Gui.GetAllTags();
                        if (selectedTag == null)
                        {
                            ACBName.Text = di.Name;
                        }
                        else
                        {
                            ACBName.Text = selectedTag;
                        }

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
                DialogsHelper.DisplayUnhandledExceptionMessage();
            }
        }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			ProcessTagging();
        }
		
		private void ProcessTagging() {
            try
            {
                if (Tagname != "")
                {
                    if (_path != "" && !FileHelper.IsZipFile(_path))
                    {
                        bool tocontinue1 = this.TriggerLongPathWarning();
						bool tocontinue2 = this.TriggerDriveWarning();

                        if (tocontinue1 && tocontinue2)
                        {
                            _main.CreateTag(Tagname);

                            TagView tv1 = null;

                            try
                            {
							    if(!_main.Gui.GetTag(Tagname).IsSyncing) {
                                    tv1 = _main.Gui.Tag(Tagname, new DirectoryInfo(_path));

                                    if (tv1 != null)
                                    {
                                        _main.InitializeTagList();
                                        _main.SelectTag(Tagname);
                                        this.Close();
                                    }
                                    else
                                    {
                                        DialogsHelper.ShowError("Tag Error", "Tag Error Occured. Please Try Again.");
                                    }
                                }
                                else
                                {
                                    DialogsHelper.ShowError(Tagname + " is Synchronizing",
                                                            "You cannot tag a folder while the tag is synchronizing.");
                                }
                            }
                            catch (Syncless.Tagging.Exceptions.RecursiveDirectoryException)
                            {
                                DialogsHelper.ShowError("Folder cannot be tagged", "Folder could not be tagged as it is a sub-folder/parent/ancestor of a folder which is already tagged.");
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
                DialogsHelper.DisplayUnhandledExceptionMessage();
            }
		}

        private bool TriggerDriveWarning()
        {
            DriveInfo di = new DriveInfo(_path);
            if (di.Name == _path)
            {
                bool result = DialogsHelper.ShowWarning("Tag Drive Warning", "You are about to tag " + _path  + "\nAre you sure you wish to continue?");

                return result;
            }

            return true;
        }

        private bool TriggerLongPathWarning()
        {
            if (_path.Length > 200)
            {
                bool result = DialogsHelper.ShowWarning("Long Path Name Warning", "NTFS File System does not handle paths which are 248 characters or more in length properly. \nAre you sure you wish to continue?");

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
            _selectedTag = ACBName.Text;
            string path = SelectPath();
            ProcessPath(path, _selectedTag);
		}

		/// <summary>
		///	Ugly Hack for Overwritting the ACB Enter Behavior because it does not accept the Enter Key
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Text_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if(e.Key == Key.Enter && _popupclosed == true) {
				BtnOk.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
			}
		}

		private void Popup_Opened(object sender, System.EventArgs e)
		{
			_popupclosed = false;
		}

		private void Popup_Closed(object sender, System.EventArgs e)
		{
			_popupclosed = true;
		}

		private void Canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			this.DragMove();
		}
    }
}
