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
        private bool _closingAnimationNotCompleted = true; // status of whether closing animation is complete
        private MainWindow _main;

        /// <summary>
        /// Initializes the ShortcutsWindow
        /// </summary>
        /// <param name="main">Reference to the Main Window</param>
        public ShortcutsWindow(MainWindow main)
        {
            InitializeComponent();
            _main = main;

            // Sets up general window properties
            Owner = _main;
            ShowInTaskbar = false;
        }

        #region Keyboard & Mouse Event handlers

        /// <summary>
        /// Any key down on the window will result it in being closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Any mouse down on the window will result it in being closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        #endregion

        #region General Window Components & Related Events

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
                e.Cancel = true;
                FormFadeOut.Begin();
            }
        }

        #endregion
    }
}