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

namespace Syncless
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IUIControllerInterface Igui;
        private List<Tag> _tagList;

        public MainWindow() {
            
            MinimizeToTray.Enable(this);

            InitializeComponent();

            InitializeSyncless();
        }
		
        /// <summary>
        ///     Starts up the system logic layer and initializes it
        /// </summary>
        private void InitializeSyncless()
        {
            Igui = ServiceLocator.GUI;
            if (Igui.Initiate()) {
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

        private void ViewTagInfo(Tag t)
        {
            TagTitle.Content = t.TagName;
			// tag.direction not implemented yet
			// tag.manual-sync mode not implemented yet
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
            _tagList = Igui.GetAllTags();

            ListBoxTag.ItemsSource = (IEnumerable) LoadTagListBoxData();
			
            UpdateTagCount();
        }
		
		private void ResetTagInfoPanel()
		{
			TagTitle.Content = "Select a Tag";
			TagIcon.Visibility = System.Windows.Visibility.Hidden;
			TagStatusPanel.Visibility = System.Windows.Visibility.Hidden;
			SyncPanel.Visibility = System.Windows.Visibility.Hidden;
			BdrTaggedPath.Visibility = System.Windows.Visibility.Hidden;
		}

        private void UpdateTagCount()
        {
            LblTagCount.Content = "[" + _tagList.Capacity + "]";
        }

        private List<string> LoadTagListBoxData()
        {
            List<string> strArray = new List<string> ();

            /*
            foreach (Tag t in _tagList)
            {
                strArray.Add(t.TagName);
            }
            */

            strArray.Add("test");
            
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

            if (Igui.PrepareForTermination())
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
                        Igui.Terminate();
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
        	if(string.Compare((string) LblDirection.Content, "Uni-Dir..") == 0)
			{
				LblDirection.Content = "Bi-Dir..";
			}
			else
			{
				LblDirection.Content = "Uni-Dir..";
			}
        }

        private void BtnSyncMode_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	if(string.Compare((string) LblSyncMode.Content, "Manual") == 0)
			{
				LblSyncMode.Content = "Auto";
				BtnPreview.Visibility = System.Windows.Visibility.Hidden;
				BtnSyncNow.Visibility = System.Windows.Visibility.Hidden;
			}
			else
			{
				LblSyncMode.Content = "Manual";
				BtnPreview.Visibility = System.Windows.Visibility.Visible;
				BtnSyncNow.Visibility = System.Windows.Visibility.Visible;
			}
        }

        private void ListBoxTag_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ListBoxTag.SelectedItem();
        }
    }
}
