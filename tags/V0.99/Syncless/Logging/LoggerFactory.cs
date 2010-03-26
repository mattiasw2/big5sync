using System;
using Syncless.Core;

namespace Syncless.Logging
{
    public class LoggerFactory
    {
        public static Logger CreateLogger(string type)
        {
            if (type.Equals(ServiceLocator.USER_LOG))
            {
                return CreateUserLog();
            }
            else if (type.Equals(ServiceLocator.DEBUG_LOG))
            {
                return CreateDebugLog();
            }
            else if (type.Equals(ServiceLocator.DEVELOPER_LOG))
            {
                return CreateDeveloperLog();
            }
            else
            {
                return null;
            }
        }
        
        private static Logger CreateUserLog()
        {
            return new UserLogger();
        }

        private static Logger CreateDebugLog()
        {
            return new DebugLogger();
        }

        private static Logger CreateDeveloperLog()
        {
            return new DeveloperLogger();
        }
    }
}
