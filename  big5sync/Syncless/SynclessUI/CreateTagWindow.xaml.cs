using System;
using System.Windows;
using System.Windows.Input;
using Syncless.Core.Exceptions;
using SynclessUI.Helper;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for CreateTagWindow.xaml
    /// </summary>
    public partial class CreateTagWindow : Window
    {
        private bool _closingAnimationNotCompleted = true; // status of whether closing animation is complete
        private MainWindow _main;

        /// <summary>
        /// Initializes the CreateTagWindow
        /// </summary>
        /// <param name="main">Reference of the Main Window</param>
        public CreateTagWindow(MainWindow main)
        {
            InitializeComponent();

            _main = main;

            // Sets up general window properties
            Owner = _main;
            ShowInTaskbar = false;
        }

        #region MyRegion
        
        /// <summary>
        /// Event handler for BtnOk_Click event. Creates tag based on specified tag name.
        /// Dialog Box closes on successful tag creation, does not close when not successful
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = false;
            try
            {
                string tagName = TxtBoxTagName.Text.Trim();

                // Check for empty tagname
                if (tagName != string.Empty)
                {
                    bool tagNotExist = _main.CreateTag(tagName);

                    if (!tagNotExist)
                    {
                        DialogHelper.ShowError(this, "Tag Already Exist", "Please specify another tagname.");
                        BtnOk.IsEnabled = true;
                    }
                    else
                    {
                        Close();
                    }
                }
                else
                {
                    DialogHelper.ShowError(this, "Tagname Empty", "Please specify a tagname.");
                    BtnOk.IsEnabled = true;
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage(this);
                Close();
            }
        }

        /// <summary>
        /// Event handler for BtnCancel_Click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsEnabled = false;
            Close();
        } 
        #endregion

        #region General Window Components & Related Events
        /// <summary>
        /// Focuses on the tag name box on load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(TxtBoxTagName);
        }

        /// <summary>
        /// Event handler for Canvas_MouseLeftButtonDown event. Allows user to drag the canvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /// <summary>
        /// On closing animation complete, set the boolean to false and closes the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            _closingAnimationNotCompleted = false;
            Close();
        }

        /// <summary>
        /// Event handler for Window_Closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_closingAnimationNotCompleted)
            {
                BtnCancel.IsCancel = false;
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        } 
        #endregion
    }
}