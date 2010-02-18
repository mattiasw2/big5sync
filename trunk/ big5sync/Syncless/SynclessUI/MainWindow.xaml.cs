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

        /// <summary>
        ///     Gets the list of tags and then populates the Tag List Box
        /// </summary>
        private void InitializeTagList()
        {
            // Gets a list of all tags
            _tagList = Igui.GetAllTags();

            ListBoxTag.ItemsSource = (IEnumerable) LoadTagListBoxData();
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
    }
}
