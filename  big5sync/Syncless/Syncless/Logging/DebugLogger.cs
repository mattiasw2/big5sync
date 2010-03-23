using System;
using System.Diagnostics;
using Syncless.Core;

namespace Syncless.Logging
{
    public class DebugLogger : Logger
    {
        public DebugLogger()
            : base(ServiceLocator.DEBUG_LOG)
        {
        }

        public override void Write(object message)
        {
            Debug.Assert(message is Exception);
            Exception e = (Exception)message;
            log.Info("------------------------------------------------");
            log.Info(e.GetType().FullName);
            log.Info("\n");
            log.Info(e.ToString());
            log.Info("------------------------------------------------");
        }
    }
}
