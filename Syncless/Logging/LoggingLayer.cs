using System;
using log4net.Config;
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
            XmlConfigurator.Configure();
        }

        public Logger GetLogger(string type)
        {
            if (type.Equals(ServiceLocator.USER_LOG))
            {
                if (userLog == null)
                {
                    CreateUserLog();
                }
                return userLog;
            }
            else
            {
                if (debugLog == null)
                {
                    CreateDebugLog();
                }
                return debugLog;
            }
        }

        private void CreateUserLog()
        {
            userLog = new Logger(ServiceLocator.USER_LOG);
        }

        private void CreateDebugLog()
        {
            debugLog = new Logger(ServiceLocator.DEBUG_LOG);
        }
    }
}
