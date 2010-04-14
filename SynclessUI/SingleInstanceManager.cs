/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 * Credits:
 * Adapted from Singleton Application
 * http://www.switchonthecode.com/tutorials/wpf-writing-a-single-instance-application
*/

using System;
using System.Linq;
using Microsoft.VisualBasic.ApplicationServices;

namespace SynclessUI
{
    /// <summary>
    /// Single Instance Manager to ensure only one Syncless App is running at one time.
    /// </summary>
    public sealed class SingleInstanceManager : WindowsFormsApplicationBase
    {
        /// <summary>
        /// Main method which creates a new SingleInstanceManager and passes in arguments.
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        public static void Main(string[] args)
        {
            (new SingleInstanceManager()).Run(args);
        }

        /// <summary>
        /// Initializes SingleInstanceManager
        /// </summary>
        public SingleInstanceManager()
        {
            IsSingleInstance = true;
        }

        /// <summary>
        /// SynclessApplication as a property
        /// </summary>
        public SynclessApplication App { get; private set; }

        #region Application Startup

        /// <summary>
        /// Overrides the OnStartup Method from Base.
        /// On first startup, creates a new Application and runs it. 
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A System.Boolean that indicates if the application should continue starting up.</returns>
        /// 
        protected override bool OnStartup(StartupEventArgs e)
        {
            App = new SynclessApplication();
            App.Run();

            return false;
        }

        /// <summary>
        /// Overrides the OnStartupNextInstance Method from Base.
        /// On subsequent startup, restore the current application instance and passes any command line arguments to it
        /// </summary>
        /// <param name="eventArgs"></param>
        protected override void OnStartupNextInstance(
            StartupNextInstanceEventArgs eventArgs)
        {
            base.OnStartupNextInstance(eventArgs);
            App.main.Activate();
            App.main.RestoreWindow();
            App.ProcessArgs(eventArgs.CommandLine.ToArray());
        }

        #endregion
    }
}