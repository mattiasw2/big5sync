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

namespace Syncless
{
    /// <summary>
    /// Interaction logic for CreateTagWindow.xaml
    /// </summary>
    public partial class CreateTagWindow : Window
    {		
		private MainWindow _main;
        
		public CreateTagWindow(MainWindow main)
        {
            InitializeComponent();
			
			_main = main;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	this.DragMove();
        }

        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			string tagName = TxtBoxTagName.Text.Trim();
			
            if(tagName != "") {
                Console.WriteLine(CmbBoxType.SelectionBoxItem.ToString());
                if (CmbBoxType.SelectionBoxItem.ToString() == "File")
                {
                    _main.CreateFileTag(tagName);
                }
                else if (CmbBoxType.SelectionBoxItem.ToString() == "Folder")
                {
					_main.CreateFolderTag(tagName);
				}
                _main.InitializeTagList();
                _main.ViewTagInfo(tagName);
				
				this.Close();
			} else {
                string messageBoxText = "Please specify a tagname.";
                string caption = "Tagname Empty";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;

                MessageBox.Show(messageBoxText, caption, button, icon);
			}            
        }
		
		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	this.Close();
        }
    }
}
