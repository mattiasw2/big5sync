using System;
using System.Collections.Generic;
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
                    userLog = LoggerFactory.CreateLogger(ServiceLocator.USER_LOG);
                }
                return userLog;
            }
            else
            {
                if (debugLog == null)
                {
                    debugLog = LoggerFactory.CreateLogger(ServiceLocator.DEBUG_LOG);
                }
                return debugLog;
            }
        }

        public List<LogData> ReadLog()
        {
            return LogReaderHelper.ReadLog();
        }
    }
}
