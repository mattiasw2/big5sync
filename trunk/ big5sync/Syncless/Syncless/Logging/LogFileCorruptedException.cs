using System;

namespace Syncless.Logging
{
    class LogFileCorruptedException : ApplicationException
    {
        public LogFileCorruptedException(string name)
            : base(name)
        {
        }
    }
}