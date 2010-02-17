using System;

namespace Syncless.Monitor.Exceptions
{
    class MonitorDriveNotFoundException : ApplicationException
    {
        private string drive;
        public string Drive
        {
            get
            {
                return drive;
            } 
        }

        public MonitorDriveNotFoundException(string drive)
            : base()
        {
            this.drive = drive;
        }

        public MonitorDriveNotFoundException(string message, string drive)
            : base(message)
        {
            this.drive = drive;
        }
    }
}