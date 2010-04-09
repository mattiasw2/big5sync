using System;

namespace Syncless.Monitor.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a drive cannot be found.
    /// </summary>
    public class MonitorDriveNotFoundException : ApplicationException
    {
        private string drive;
        /// <summary>
        /// Gets a value for the drive name.
        /// </summary>
        public string Drive
        {
            get
            {
                return drive;
            } 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Monitor.Exceptions.MonitorDriveNotFoundException" /> class, given the drive name.
        /// </summary>
        /// <param name="drive">A <see cref="string" /> specifying the drive name.</param>
        public MonitorDriveNotFoundException(string drive)
            : base()
        {
            this.drive = drive;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Monitor.Exceptions.MonitorDriveNotFoundException" /> class, given the message for this exception and the drive name.
        /// </summary>
        /// <param name="message">A <see cref="string" /> specifying the message for this exception.</param>
        /// <param name="drive">A <see cref="string" /> specifying the drive name.</param>
        public MonitorDriveNotFoundException(string message, string drive)
            : base(message)
        {
            this.drive = drive;
        }
    }
}