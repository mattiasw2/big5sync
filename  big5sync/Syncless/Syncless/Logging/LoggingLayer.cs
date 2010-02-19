using System;
using Syncless.Core;

namespace Syncless.Logging
{
    public class LoggingLayer
    {
        private static LoggingLayer _instance;
        public static LoggingLayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LoggingLayer();
                }
                return _instance;
            }
        }

        private Logger userLog;
        private Logger debugLog;

        private LoggingLayer()
        {
            userLog = Logger.GetInstance(ServiceLocator.USER_LOG);
            debugLog = Logger.GetInstance(ServiceLocator.DEBUG_LOG);
        }

        public Logger GetLogger(string type)
        {
            if (type.Equals(ServiceLocator.USER_LOG))
            {
                return userLog;
            }
            else
            {
                return debugLog;
            }
        }
    }
}
