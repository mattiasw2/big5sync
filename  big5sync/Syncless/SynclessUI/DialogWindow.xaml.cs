using System;
using System.ComponentModel;
using System.Drawing;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SynclessUI.Helper;
using SynclessUI.Properties;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        private bool _closingAnimationNotCompleted = true;

        public DialogWindow(Window parentWindow, string caption, string message, DialogType dt)
        {
            InitializeComponent();
            CannotBeClosed = false;
			
			try {				
				if(dt != DialogType.Indeterminate) {
					this.ShowInTaskbar = false;
					this.Owner = parentWindow;
				}
			} catch(InvalidOperationException) {
			}
					
            Application.Current.Properties["DialogWindowChoice"] = false;
            LblCaption.Content = caption;
            TxtBlkMessageBoxText.Text = message;

            StyleDialogBox(dt);
            PlayDialogSound(dt);
        }

        public bool CannotBeClosed { get; set; }

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
                    break;
            }

            ImgIcon.Source = GetSystemImage(dt);
        }

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
            }
        }

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

        private void BtnOkCP1_Click(object sender, RoutedEventArgs e)
        {
            BtnOkCP1.IsEnabled = false;
            Close();
        }

        private void BtnOkCP2_Click(object sender, RoutedEventArgs e)
        {
            BtnOkCP2.IsEnabled = false;
            Application.Current.Properties["DialogWindowChoice"] = true;
            Close();
        }

        private void BtnCancelCP2_Click(object sender, RoutedEventArgs e)
        {
            BtnCancelCP2.IsEnabled = false;
            Application.Current.Properties["DialogWindowChoice"] = false;
            Close();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            _closingAnimationNotCompleted = false;
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (CannotBeClosed)
                e.Cancel = true;

            if (_closingAnimationNotCompleted)
            {
                BtnOkCP1.IsCancel = false;
                BtnCancelCP2.IsCancel = false;
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
        }
    }
}