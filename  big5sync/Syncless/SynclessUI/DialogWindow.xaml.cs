/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SynclessUI.Helper;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml. This class is the generic customized dialog window, where components
    /// are shown or not depending on the dialog type
    /// </summary>
    public partial class DialogWindow : Window
    {
        private bool _closingAnimationNotCompleted = true; // status of whether closing animation is complete

        /// <summary>
        /// Initializes the DialogWindow
        /// </summary>
        /// <param name="parentWindow">Reference to the parent window</param>
        /// <param name="caption">Caption to Show in Dialog Box</param>
        /// <param name="message">Messageto Show in Dialog Box</param>
        /// <param name="dt">Enumerated type - Dialog Type</param>
        public DialogWindow(Window parentWindow, string caption, string message, DialogType dt)
        {
            InitializeComponent();

            // Resets Choice for Warning DialogBoxType
            Application.Current.Properties["DialogWindowChoice"] = false;

            // Sets up window properties for Indeterminate Dialog Box Type
            try
            {
                if (dt != DialogType.Indeterminate)
                {
                    ShowInTaskbar = false;
                    Owner = parentWindow;
                    CannotBeClosed = false;
                }
                else
                {
                    CannotBeClosed = true;
                }
            }
            catch (InvalidOperationException)
            {
            }

            // Sets the caption & message of the Dialog Box
            LblCaption.Content = caption;
            TxtBlkMessageBoxText.Text = message;

            // Styles the Dialog Box
            StyleDialogBox(dt);
            PlayDialogSound(dt);
        }

        #region Dialog Box Customization

        /// <summary>
        /// Styles the Dialog Box (Components Visibility etc.) according to the dialog box type
        /// </summary>
        /// <param name="dt">Enumerated type - Dialog Type</param>
        private void StyleDialogBox(DialogType dt)
        {
            switch (dt)
            {
                case DialogType.Error:
                    Title = "Error";
                    OkCommandPanel.Visibility = Visibility.Visible;
                    break;
                case DialogType.Warning:
                    Title = "Warning";
                    OkCancelCommandPanel.Visibility = Visibility.Visible;
                    break;
                case DialogType.Information:
                    Title = "Information";
                    OkCommandPanel.Visibility = Visibility.Visible;
                    break;
                case DialogType.Indeterminate:
                    CannotBeClosed = true;
                    Title = (string) LblCaption.Content;
                    ProgressBarTermination.IsEnabled = true;
                    ProgressBarTermination.Visibility = Visibility.Visible;
                    MinimizePanel.Visibility = Visibility.Visible;
                    break;
            }

            // Gets the approriate image type to show besides dialog message
            ImgIcon.Source = GetSystemImage(dt);
        }

        /// <summary>
        /// Plays the approriate dialog sound according to the Dialog Type. Indeterminate type has no sound
        /// </summary>
        /// <param name="dt"></param>
        private static void PlayDialogSound(DialogType dt)
        {
            switch (dt)
            {
                case DialogType.Error:
                    SystemSounds.Beep.Play();
                    break;
                case DialogType.Warning:
                    SystemSounds.Exclamation.Play();
                    break;
                case DialogType.Information:
                    SystemSounds.Exclamation.Play();
                    break;
            }
        }

        /// <summary>
        /// Retrieves the system icons as an ImageSource based on DialogType
        /// </summary>
        /// <param name="dt">Enumerated type - Dialog Type</param>
        /// <returns></returns>
        private static ImageSource GetSystemImage(DialogType dt)
        {
            Icon iconsource = null;
            switch (dt)
            {
                case DialogType.Error:
                    iconsource = SystemIcons.Error;
                    break;
                case DialogType.Warning:
                    iconsource = SystemIcons.Exclamation;
                    break;
                case DialogType.Information:
                    iconsource = SystemIcons.Information;
                    break;
                case DialogType.Indeterminate:
                    iconsource = SystemIcons.Exclamation;
                    break;
            }
            if (iconsource == null) return null;
            else
                return Imaging.CreateBitmapSourceFromHIcon(iconsource.Handle, Int32Rect.Empty,
                                                           BitmapSizeOptions.FromEmptyOptions());
        }

        #endregion

        #region Command Panel

        /// <summary>
        /// Gets and Sets whether the Dialog Box can be closed or not.
        /// </summary>
        public bool CannotBeClosed { get; set; }

        /// <summary>
        /// Command Panel - Ok Button for Information/Error Dialog Types
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOkCP1_Click(object sender, RoutedEventArgs e)
        {
            BtnOkCP1.IsEnabled = false;
            Close();
        }

        /// <summary>
        /// Command Panel - Ok Button for Warning Dialog Type. Sets the chosen choice to true
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOkCP2_Click(object sender, RoutedEventArgs e)
        {
            BtnOkCP2.IsEnabled = false;
            Application.Current.Properties["DialogWindowChoice"] = true;
            Close();
        }

        /// <summary>
        /// Command Panel - Cancel Button for Warning Dialog Type. Sets the chosen choice to false
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancelCP2_Click(object sender, RoutedEventArgs e)
        {
            BtnCancelCP2.IsEnabled = false;
            Application.Current.Properties["DialogWindowChoice"] = false;
            Close();
        }

        #endregion

        #region General Window Components & Related Events

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
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (CannotBeClosed)
            {
                e.Cancel = true;
                return;
            }

            if (_closingAnimationNotCompleted)
            {
                BtnOkCP1.IsCancel = false;
                BtnCancelCP2.IsCancel = false;
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        }

        /// <summary>
        /// Event handler for BtnMin_MouseLeftButtonDown. Minimizes the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMin_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        #endregion
    }
}