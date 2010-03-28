using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for ShortcutsWindow.xaml
    /// </summary>
    public partial class ShortcutsWindow : Window
    {
		public ShortcutsWindow()
        {
            InitializeComponent();
        }

		private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
            CloseWindow();
		}

		private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
            CloseWindow();
		}
		
		private void CloseWindow() {
            FormFadeOut.Begin();
		}

        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
