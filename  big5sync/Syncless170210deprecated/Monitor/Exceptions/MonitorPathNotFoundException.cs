using System;

namespace Syncless.Monitor.Exceptions
{
    class MonitorPathNotFoundException : ApplicationException
    {
        private string path;
        public string Path
        {
            get
            {
                return path;
            } 
        }

        public MonitorPathNotFoundException(string path)
            : base()
        {
            this.path = path;
        }

        public MonitorPathNotFoundException(string message, string path)
            : base(message)
        {
            this.path = path;
        }
    }
}
