using System;
using System.Diagnostics;
using System.Text;
using Syncless.Core;

namespace Syncless.Logging
{
    public class DebugLogger : Logger
    {
        public DebugLogger()
            : base(ServiceLocator.DEBUG_LOG)
        {
        }

        public override void Write(object message)
        {
            Debug.Assert(message is Exception);
            Exception e = (Exception)message;
            string linebreak = "--------------------------------------------------------";
            string indent = "\t\t\t";
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Exception Caught");
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
