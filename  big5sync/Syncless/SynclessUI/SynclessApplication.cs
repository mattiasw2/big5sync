using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SynclessUI
{
    public class SynclessApplication : Application
    {
        public Window main { get; private set; }

        public SynclessApplication(): base()
        { }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Application.Current.Resources.MergedDictionaries.Add(

            Application.LoadComponent(new Uri("SynclessUI;component/AppResourceDictionary.xaml", UriKind.Relative)) as ResourceDictionary);

            main = new MainWindow();
            ProcessArgs(e.Args, true);

            main.Show();
        }

        public void ProcessArgs(string[] args, bool firstInstance)
        {
        }
    }
}
