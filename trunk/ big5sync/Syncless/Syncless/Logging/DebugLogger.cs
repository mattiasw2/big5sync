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
            log.Info(message);
        }
    }
}
