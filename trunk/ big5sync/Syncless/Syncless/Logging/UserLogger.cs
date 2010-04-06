using System;
using System.Diagnostics;
using System.Text;
using Syncless.Core;

namespace Syncless.Logging
{
    public class UserLogger : Logger
    {
        private const string delimiter = " ~:~ ";

        public UserLogger()
            : base(ServiceLocator.USER_LOG)
        {
        }

        public override void Write(object message)
        {
            Debug.Assert(message is LogData);
            LogData logData = message as LogData;
            StringBuilder builder = new StringBuilder();
            builder.Append(logData.LogCategory.ToString());
            builder.Append(delimiter);
            builder.Append(logData.LogEvent.ToString());
            builder.Append(delimiter);
            builder.Append(logData.Message);
            log.Info(builder.ToString());
        }
    }
}
