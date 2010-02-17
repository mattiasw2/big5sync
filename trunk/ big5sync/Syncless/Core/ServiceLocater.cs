using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Core
{
    public class ServiceLocator
    {
        
        public static IUIControllerInterface GUI{
            get { return SystemLogicLayer.Instance; }
        }
        public static IMonitorControllerInterface MonitorI
        {
            get { return SystemLogicLayer.Instance; }
        }
        /*
        public static ICommandLineControllerInterface CommandLine
        {
            get { return SystemLogicLayer.Instance; }
        }
        */
        /*
        public static ILoggerInterface Logging
        {
            get { return new Logger(); }
        }
        */
    }
}
