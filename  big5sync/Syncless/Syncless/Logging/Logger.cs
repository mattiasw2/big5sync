using System;
using log4net;
using Syncless.Core;

namespace Syncless.Logging
{
    /// <summary>
    /// An abstract class for the different logger for the system
    /// </summary>
    public abstract class Logger
    {
        /// <summary>
        /// the logger from log4net
        /// </summary>
        protected ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Logging.Logger" /> class, given the specified name.
        /// </summary>
        /// <param name="name">A <see cref="string" /> specifying the deleted path.</param>
        public Logger(string name)
        {
            log = LogManager.GetLogger(name);
        }
        
        /// <summary>
        /// Write to logger
        /// </summary>
        /// <param name="message">the object that the inherited logger required</param>
        public abstract void Write(object message);
    }
}
