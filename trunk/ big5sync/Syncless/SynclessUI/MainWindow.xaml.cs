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
using SynclessUI;
using System.Data;
using Syncless.Core;
using Syncless.Tagging;
using System.Collections;
using System.IO;

namespace Syncless
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IUIControllerInterface _Igui;
        private List<Tag> _tagList;
        private Tag _selectedTag;

        private const string BI_DIRECTIONAL = "Bi-Dir..";
        private const string UNI_DIRECTIONAL = "Uni-Dir..";

        public MainWindow() {
            
            MinimizeToTray.Enable(this);

            InitializeComponent();

            // InitializeSyncless();
        }
		
        /// <summary>
        ///     Starts up the system logic layer and initializes it
        /// </summary>
        private void InitializeSyncless()
        {
            _Igui = ServiceLocator.GUI;
            if (_Igui.Initiate()) {
                InitializeTagList();
				ResetTagInfoPanel();
            }
            else {
                string messageBoxText = "Syncless has failed to initialize and will now exit.";
                string caption = "Syncless Initialization Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);

                this.Close();
            }
        }

        private void ListBoxTag_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedTag = null;
            
            String neededTagName = ListBoxTag.SelectedItem.ToString();

            foreach (Tag temp in _tagList)
            {
                if (string.Compare(neededTagName, temp.TagName) == 0)
                {
                    _selectedTag = temp;
                    break;
                }
            }

            ViewTagInfo(_selectedTag);
        }

        private void ViewTagInfo(Tag t)
        {
            TagTitle.Content = t.TagName;
			// tag.direction not implemented yet
			
            if (t.IsSeamless)
            {
                AutoMode();
            }
            else
            {
                ManualMode();
            }

            LblStatusText.Content = "";
			ListTaggedPath.ItemsSource = t.PathStringList;
			
			if(t is FileTag) {
				var uriSource = new Uri(@"/SynclessUI;component/Icons/file.ico", UriKind.Relative);
				TagIcon.Source = new BitmapImage(uriSource);
			} else {
				var uriSource = new Uri(@"/SynclessUI;component/Icons/folder.ico", UriKind.Relative);
				TagIcon.Source = new BitmapImage(uriSource);
			}
			
			TagIcon.Visibility = System.Windows.Visibility.Visible;
			TagStatusPanel.Visibility = System.Windows.Visibility.Visible;
			SyncPanel.Visibility = System.Windows.Visibility.Visible;
			BdrTaggedPath.Visibility = System.Windows.Visibility.Visible;
        }
		
        /// <summary>
        ///     Gets the list of tags and then populates the Tag List Box
        /// </summary>
        private void InitializeTagList()
        {
            // Gets a list of all tags
            _tagList = _Igui.GetAllTags();

            ListBoxTag.ItemsSource = LoadTagListBoxData();
			
            UpdateTagCount();
        }
		
        /// <summary>
        ///     If lists of tag is empty, reset the UI back to 0, else displayed the first Tag on the list.
        /// </summary>
		private void ResetTagInfoPanel()
		{
            if (_tagList.Count == 0)
            {
                TagTitle.Content = "Select a Tag";
                TagIcon.Visibility = System.Windows.Visibility.Hidden;
                TagStatusPanel.Visibility = System.Windows.Visibility.Hidden;
                SyncPanel.Visibility = System.Windows.Visibility.Hidden;
                BdrTaggedPath.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                ViewTagInfo(_tagList[0]);
            }
		}

        private void UpdateTagCount()
        {
            LblTagCount.Content = "[" + _tagList.Count + "]";
        }

        private List<string> LoadTagListBoxData()
        {
            List<string> strArray = new List<string> ();

            foreach (Tag t in _tagList)
            {
                strArray.Add(t.TagName);
            }
            
            return strArray;
        }

        /// <summary>
        ///     Makes the title bar draggable and movable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }

        /// <summary>
        ///     Sets the behavior of the close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Prepares the SLL for termination

            if(_Igui.PrepareForTermination())
            {
                string messageBoxText = "Are you sure you want to exit Syncless?" +
                    "\nExiting Syncless will disable seamless synchronization.";
                string caption = "Exit";
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;

                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        // Terminates the SLL and closes the UI
                        _Igui.Terminate();
                        this.Close();
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
            else
            {
                string messageBoxText = "Syncless is not ready for termination. Please try again later";
                string caption = "Syncless Termination Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);
            }
        }

        /// <summary>
        ///     Sets the behavior of the minimize button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMin_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
			this.WindowState = WindowState.Minimized;
        }

        private void TxtBoxFilterTag_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
        	TxtBoxFilterTag.Text = "";
        }

        private void TxtBoxFilterTag_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
        	TxtBoxFilterTag.Text = "Filter";
        }

        private void BtnDirection_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	if(string.Compare((string) LblDirection.Content, UNI_DIRECTIONAL) == 0)
			{
				LblDirection.Content = BI_DIRECTIONAL;
			}
			else
			{
                LblDirection.Content = UNI_DIRECTIONAL;
			}
        }

        private void BtnSyncMode_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	if(string.Compare((string) LblSyncMode.Content, "Manual") == 0)
			{
                if (_Igui.MonitorTag(_selectedTag, true))
                {
                    AutoMode();
                }
                else
                {
                    string messageBoxText = "' " + _selectedTag.TagName +" ' could not be set into Seamless Mode.";
                    string caption = "Change Synchronization Mode Error";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxText, caption, button, icon);
                }
			}
			else
			{
                if (_Igui.MonitorTag(_selectedTag, false))
                {
                    ManualMode();
                }
                else
                {
                    string messageBoxText = "' " + _selectedTag.TagName + " ' could not be set into Manual Mode.";
                    string caption = "Change Synchronization Mode Error";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxText, caption, button, icon);
                }
			}
        }

        private void AutoMode() {
            LblSyncMode.Content = "Auto";
            BtnPreview.Visibility = System.Windows.Visibility.Hidden;
            BtnSyncNow.Visibility = System.Windows.Visibility.Hidden;
        }

        private void ManualMode()
        {
            LblSyncMode.Content = "Manual";
            BtnPreview.Visibility = System.Windows.Visibility.Visible;
            BtnSyncNow.Visibility = System.Windows.Visibility.Visible;
        }

        private void BtnSyncNow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	if(!_Igui.StartManualSync(_selectedTag)) {
                string messageBoxText = "' " + _selectedTag.TagName + " ' could not be synchronized.";
                string caption = "Synchronization Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);
			}
        }

        private void btnRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string messageBoxText = "Are you sure you want to delete the tag '" + _selectedTag.TagName + "'?";
            string caption = "Delete Tag";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    if (_Igui.DeleteTag(_selectedTag))
                    {
                        InitializeTagList();
                        ResetTagInfoPanel();
                    }
                    else
                    {
                        messageBoxText = "' " + _selectedTag.TagName + " ' could not be deleted.";
                        caption = "Delete Tag Error";
                        button = MessageBoxButton.OK;
                        icon = MessageBoxImage.Error;

                        MessageBox.Show(messageBoxText, caption, button, icon);
                    }
                    break;
                case MessageBoxResult.No:
                    break;
            }            
        }
		
		public void CreateFileTag(string tagName) {
			Tag t = _Igui.CreateFileTag(tagName);
			if(t != null) {
				InitializeTagList();
				_selectedTag = t;
				ViewTagInfo(_selectedTag);
			} else {
                string messageBoxText = "File Tag could not be created.";
                string caption = "File Tag Creation Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);
			}
		}
		
		public void CreateFolderTag(string tagName) {
			Tag t =_Igui.CreateFolderTag(tagName);
			if(t != null) {
				InitializeTagList();
				_selectedTag = t;
				ViewTagInfo(_selectedTag);
			} else {
                string messageBoxText = "Folder Tag could not be created.";
                string caption = "Folder Tag Creation Error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);
			}
		}

		private void BtnCreate_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			CreateTagWindow ctw = new CreateTagWindow(this);
			
			ctw.Show();
		}

		private void btnTag_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            if(_selectedTag != null) {
                Tag t = null;

                if(_selectedTag is FileTag) {	
                    Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
					
                    ofd.Filter = "All Files(*.*)|*.*";
					
                    ofd.ShowDialog();
                    string path = ofd.FileName;

                    t = _Igui.TagFile((FileTag)_selectedTag, new FileInfo(path));
                } else if(_selectedTag is FolderTag) {
                    System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
                    folderDialog.Description = "Select Folder to Tag";

                    folderDialog.ShowDialog();

                    if(folderDialog.SelectedPath != string.Empty) {	
                        string path = folderDialog.SelectedPath;
                        t =_Igui.TagFolder((FolderTag)_selectedTag, new DirectoryInfo(path));
                    }
                    
                }

                if (t != null)
                {
                    InitializeTagList();
                    ViewTagInfo(t);
                }
                else
                {
                    string messageBoxText = "Tagging has fail.";
                    string caption = "Tagging Unsuccessful";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;

                    MessageBox.Show(messageBoxText, caption, button, icon);
                }
            } else {
                string messageBoxText = "Please select a file/folder tag.";
                string caption = "No Tag Selected";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);
            }
        }
    }
}
