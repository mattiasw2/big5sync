using System;
using System.Windows;
using System.Windows.Input;

namespace SynclessUI
{
    /// <summary>
    /// Interaction logic for ShortcutsWindow.xaml
    /// </summary>
    public partial class ShortcutsWindow : Window
    {
        private bool _closingAnimationNotCompleted = true;
        private MainWindow _main;

        public ShortcutsWindow(MainWindow main)
        {
            InitializeComponent();
            _main = main;
            Owner = _main;
            ShowInTaskbar = false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            _closingAnimationNotCompleted = false;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_closingAnimationNotCompleted)
            {
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        }

        private void LayoutRoot_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
        	if(e.Key == Key.Escape) {
				Close();
			}
        }
    }
}