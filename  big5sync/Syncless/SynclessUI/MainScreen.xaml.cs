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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Syncless
{
    /// <summary>
    /// Interaction logic for MainScreen.xaml
    /// </summary>
    public partial class MainScreen : Window
    {
        public MainScreen()
        {
            InitializeComponent();
        }

        private void btnRemoveTag_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			if(TagListBox.SelectedItem != null) {
				string messageBoxText = "Are you sure you want to remove this tag?";
				string caption = "Remove Tag";
				MessageBoxButton button = MessageBoxButton.YesNo;
				MessageBoxImage icon = MessageBoxImage.Warning;
	
				MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);
	
				switch (result) {
					case MessageBoxResult.Yes:
						TagListBox.Items.RemoveAt(TagListBox.Items.IndexOf(TagListBox.SelectedItem));
						break;
					case MessageBoxResult.No:
						break;
				}
			} else {
				// MessageBox.Show("No Tag Selected!", "Remove Tag", MessageBoxButton.OK, MessageBoxImage.Error);
			}
        }
    }
}
