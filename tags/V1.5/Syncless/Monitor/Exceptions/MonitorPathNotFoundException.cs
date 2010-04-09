using System;

namespace Syncless.Monitor.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a path to be monitored cannot be found.
    /// </summary>
    public class MonitorPathNotFoundException : ApplicationException
    {
        private string path;
        /// <summary>
        /// Gets a value for the path.
        /// </summary>
        public string Path
        {
            get
            {
                return path;
            } 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Monitor.Exceptions.MonitorDriveNotFoundException" /> class, given the path.
        /// </summary>
        /// <param name="path">A <see cref="string" /> specifying the path.</param>
        public MonitorPathNotFoundException(string path)
            : base()
        {
            this.path = path;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Monitor.Exceptions.MonitorDriveNotFoundException" /> class, given the message for this exception and the path.
        /// </summary>
        /// <param name="message">A <see cref="string" /> specifying the message for this exception.</param>
        /// <param name="path">A <see cref="string" /> specifying the path.</param>
        public MonitorPathNotFoundException(string message, string path)
            : base(message)
        {
            this.path = path;
        }
    }
}
