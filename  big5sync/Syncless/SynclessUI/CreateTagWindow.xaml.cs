using System;
using System.ComponentModel;
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
        private readonly MainWindow _main;
        private bool _closingAnimationNotCompleted = true;

        public CreateTagWindow(MainWindow main)
        {
            InitializeComponent();

            _main = main;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnOk.IsEnabled = false;
            try
            {
                string tagName = TxtBoxTagName.Text.Trim();

                if (tagName != "")
                {
                    bool tagexists = false;

                    tagexists = _main.CreateTag(tagName);

                    if (!tagexists)
                    {
                        DialogHelper.ShowError("Tag Already Exist", "Please specify another tagname.");
                        BtnOk.IsEnabled = true;
                    }
                    else
                    {
                        Close();
                    }
                }
                else
                {
                    DialogHelper.ShowError("Tagname Empty", "Please specify a tagname.");
                    BtnOk.IsEnabled = true;
                }
            }
            catch (UnhandledException)
            {
                DialogHelper.DisplayUnhandledExceptionMessage();
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            BtnCancel.IsEnabled = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(TxtBoxTagName);
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
            if (_closingAnimationNotCompleted)
            {
                BtnCancel.IsCancel = false;
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        }
    }
}