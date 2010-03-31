using System;
using System.Diagnostics;
using System.Text;
using Syncless.Core;

namespace Syncless.Logging
{
    public class DeveloperLogger : Logger
    {

        public DeveloperLogger()
            : base(ServiceLocator.DEVELOPER_LOG)
        {
        }

        public override void Write(object message)
        {
            Debug.Assert(message is String);
            string msg = message as String;
            log.Debug(msg);
        }
    }
}
