using System;
using System.Diagnostics;
using Syncless.Core;

namespace Syncless.Logging
{
    public class UserLogger : Logger
    {
        public UserLogger()
            : base(ServiceLocator.USER_LOG)
        {
        }

        public override void Write(object message)
        {
            Debug.Assert(message is object);
            log.Info(message);
        }
    }
}
