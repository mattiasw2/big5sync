using System;
using System.Diagnostics;
using System.Text;
using Syncless.Core;

namespace Syncless.Logging
{
    /// <summary>
    /// This class will write the debug log, which records any unhandled exception
    /// </summary>
    public class DebugLogger : Logger
    {
        private const string linebreak = "--------------------------------------------------------";
        private const string indent = "\t\t\t";

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Logging.DebugLogger" /> class.
        /// </summary>
        public DebugLogger()
            : base(ServiceLocator.DEBUG_LOG)
        {
        }

        /// <summary>
        /// Write to logger
        /// </summary>
        /// <param name="message">the object must be of type <see cref="System.Exception"/>.</param>
        public override void Write(object message)
        {
            Debug.Assert(message is Exception);
            Exception e = message as Exception;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("\tException Caught");
            builder.Append(indent);
            builder.AppendLine(linebreak);
            builder.Append(indent);
            builder.AppendLine(e.GetType().FullName);
            builder.AppendLine("\n");
            builder.Append(indent);
            string[] stacktraces = e.ToString().Split(new char[] { '\n' });
            foreach (string trace in stacktraces)
            {
                builder.Append(indent);
                builder.AppendLine(trace);
            }
            builder.Append(indent);
            builder.AppendLine(linebreak);
            log.Info(builder.ToString());
        }
    }
}
