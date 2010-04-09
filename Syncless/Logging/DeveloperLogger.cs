using System;
using System.Diagnostics;
using System.Text;
using Syncless.Core;

namespace Syncless.Logging
{
    /// <summary>
    /// This class will write the developer log, which facilitate the developer during development, testing and debugging.
    /// </summary>
    public class DeveloperLogger : Logger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Logging.DeveloperLogger" /> class.
        /// </summary>
        public DeveloperLogger()
            : base(ServiceLocator.DEVELOPER_LOG)
        {
        }

        /// <summary>
        /// Write to logger
        /// </summary>
        /// <param name="message">the object must be of type <see cref="string"/>.</param>
        public override void Write(object message)
        {
            Debug.Assert(message is String);
            string msg = message as String;
            log.Debug(msg);
        }
    }
}
