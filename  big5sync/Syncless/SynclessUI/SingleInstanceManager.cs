/*
 * Credits:
 * Adapted from Singleton Application
 * http://www.switchonthecode.com/tutorials/wpf-writing-a-single-instance-application
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.ApplicationServices;

namespace SynclessUI
{
    public sealed class SingleInstanceManager : WindowsFormsApplicationBase
    {
        [STAThread]
        public static void Main(string[] args)
        { (new SingleInstanceManager()).Run(args); }

        public SingleInstanceManager()
        { IsSingleInstance = true; }

        public SynclessApplication App { get; private set; }

        protected override bool OnStartup(StartupEventArgs e)
        {
            App = new SynclessApplication();
            App.Run();

            /* YC: Sorry for editing your code, was testing out threading stuff
            Thread t = new Thread(StartUIThread);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();*/

            return false;
        }

        /* YC
        public void StartUIThread()
        {
            App = new SynclessApplication();
            App.Run();
        }*/

        protected override void OnStartupNextInstance(
          StartupNextInstanceEventArgs eventArgs)
        {
            base.OnStartupNextInstance(eventArgs);
            App.main.Activate();
            App.main.RestoreWindow();
            App.ProcessArgs(eventArgs.CommandLine.ToArray());
        }
    }
}