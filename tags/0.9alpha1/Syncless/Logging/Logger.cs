using System;
using log4net;
using Syncless.Core;

namespace Syncless.Logging
{
    public class Logger
    {
        private ILog log;

        public Logger(string name)
        {
            log = LogManager.GetLogger(name);
        }

        public void WriteLine(string message)
        {
            log.Info(message);
        }
    }
}
