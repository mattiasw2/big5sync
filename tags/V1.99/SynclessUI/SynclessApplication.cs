using System;
using System.Windows;

namespace SynclessUI
{
    /// <summary>
    /// SynclessApplication is the Application class which calls on the MainWindow everytime it is initialized.
    /// Adapted from Singleton Application
    /// http://www.switchonthecode.com/tutorials/wpf-writing-a-single-instance-application
    /// </summary>

    public class SynclessApplication : Application
    {
        public MainWindow main { get; private set; }

        /// <summary>
        /// Initializes SynclessApplication
        /// </summary>
        public SynclessApplication() : base()
        {
        }

        /// <summary>
        /// On Application Startup, Load Resource Dictionary and creates the Main Window
        /// </summary>
        /// <param name="e">Arguments Passed Into The Program. Eg. Commandline</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Application.Current.Resources.MergedDictionaries.Add(
                Application.LoadComponent(new Uri("SynclessUI;component/AppResourceDictionary.xaml", UriKind.Relative))
                as ResourceDictionary);

            main = new MainWindow();

            ProcessArgs(e.Args);
        }

        /// <summary>
        /// Calls on the MainWindow to process all commandline arguments
        /// </summary>
        /// <param name="args"></param>
        public void ProcessArgs(string[] args)
        {
            main.ProcessCommandLine(args);
        }
    }
}