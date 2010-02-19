using System;
using log4net;
using Syncless.Core;

namespace Syncless.Logging
{
    public class Logger
    {
        private static Logger _instance;
        private ILog log;

        private Logger(string name)
        {
            log = LogManager.GetLogger(ServiceLocator.USER_LOG);
            
        }

        public static Logger GetInstance(string name)
        {
            if (_instance == null)
            {
                _instance = new Logger(name);
            }
            return _instance;
        }
    }
}
