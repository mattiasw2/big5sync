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
        return false;
    }

    protected override void OnStartupNextInstance(
      StartupNextInstanceEventArgs eventArgs)
    {
        base.OnStartupNextInstance(eventArgs);
        App.main.Activate();
        App.ProcessArgs(eventArgs.CommandLine.ToArray(), false);
    }
}
}