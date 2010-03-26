using System.Windows;

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
			this.Close();
		}

		private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			this.Close();
		}

		private void LayoutRoot_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			this.Close();
		}
    }
}
