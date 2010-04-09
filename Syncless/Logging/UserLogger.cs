using System;
using System.Diagnostics;
using System.Text;
using Syncless.Core;

namespace Syncless.Logging
{
    /// <summary>
    /// This class will write the user log, which records the actions of the system performed by the user
    /// </summary>
    public class UserLogger : Logger
    {
        private const string delimiter = " ~:~ ";

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Logging.UserLogger" /> class.
        /// </summary>
        public UserLogger()
            : base(ServiceLocator.USER_LOG)
        {
        }

        /// <summary>
        /// Write to logger
        /// </summary>
        /// <param name="message">the object must be of type <see cref="Syncless.Logging.LogData"/>.</param>
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
