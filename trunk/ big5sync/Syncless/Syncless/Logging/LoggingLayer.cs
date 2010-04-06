using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private Logger developerLog;

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
            else if (type.Equals(ServiceLocator.DEBUG_LOG))
            {
                if (debugLog == null)
                {
                    debugLog = LoggerFactory.CreateLogger(ServiceLocator.DEBUG_LOG);
                }
                return debugLog;
            }
            else if (type.Equals(ServiceLocator.DEVELOPER_LOG))
            {
                if (developerLog == null)
                {
                    developerLog = LoggerFactory.CreateLogger(ServiceLocator.DEVELOPER_LOG);
                }
                return developerLog;
            }
            else
            {
                Debug.Assert(false);
                return null;
            }
        }

        public List<LogData> ReadLog()
        {
            return LogReaderHelper.ReadLog();
        }

        public void ClearLog()
        {
            File.Delete(LogReaderHelper.USER_LOG_BACKUP_PATH);
            File.Delete(LogReaderHelper.USER_LOG_PATH);
        }
    }
}
