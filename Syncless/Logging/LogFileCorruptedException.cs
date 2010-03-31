using System;

namespace Syncless.Logging
{
    public class LogFileCorruptedException : ApplicationException
    {
        public LogFileCorruptedException(string name)
            : base(name)
        {
        }
    }
}