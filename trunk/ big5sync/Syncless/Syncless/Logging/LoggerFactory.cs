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
            else
            {
                return CreateDebugLog();
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
    }
}
