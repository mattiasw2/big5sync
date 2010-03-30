using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Media;
using SynclessUI.Helper;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public bool CannotBeClosed {
			get; set;
		}

        public DialogWindow(string caption, string message, DialogType dt)
        {
            InitializeComponent();
			CannotBeClosed = false;
			Application.Current.Properties["DialogWindowChoice"] = false;
            LblCaption.Content = caption;
            TxtBlkMessageBoxText.Text = message;

			StyleDialogBox(dt);
            PlayDialogSound(dt);
        }

        private void StyleDialogBox(DialogType dt)
        {
			switch (dt)
			{
				case DialogType.Error:
					this.Title = "Error";
				    this.OkCommandPanel.Visibility = System.Windows.Visibility.Visible;
					break;
                case DialogType.Warning:
					this.Title = "Warning";
					this.OkCancelCommandPanel.Visibility = System.Windows.Visibility.Visible;
				    break;
                case DialogType.Information:
					this.Title = "Information";
					this.OkCommandPanel.Visibility = System.Windows.Visibility.Visible;
				    break;
                case DialogType.Indeterminate:
					CannotBeClosed = true;
			        this.Title = (string) LblCaption.Content;
					this.ProgressBarTermination.IsEnabled = true;
					this.ProgressBarTermination.Visibility = System.Windows.Visibility.Visible;
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
			System.Drawing.Icon iconsource = null;
			switch (dt)
			{
				case DialogType.Error:
					iconsource = System.Drawing.SystemIcons.Error;
				    break;
				case DialogType.Warning:
					iconsource = System.Drawing.SystemIcons.Exclamation;
				    break;
				case DialogType.Information:
					iconsource = System.Drawing.SystemIcons.Information;
				    break;
                case DialogType.Indeterminate:
                    iconsource = System.Drawing.SystemIcons.Exclamation;
                    break;
			}
			if (iconsource == null) return null;
				else return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(iconsource.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
		}

        private void BtnOkCP1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			BtnOkCP1.IsEnabled = false;
        	CloseWindow();
        }
		
        private void BtnOkCP2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			BtnOkCP2.IsEnabled = false;
			Application.Current.Properties["DialogWindowChoice"] = true;
        	CloseWindow();
        }
		
        private void BtnCancelCP2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			BtnCancelCP2.IsEnabled = false;
			Application.Current.Properties["DialogWindowChoice"] = false;
        	CloseWindow();
        }

        private void Canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }
		
		public void CloseWindow() {
            FormFadeOut.Begin();
		}
		
        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        	if(CannotBeClosed)
				e.Cancel = true;
        }
    }
}
