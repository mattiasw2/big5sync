using System;

namespace Syncless.Core.Exceptions
{
    class LoggerNotFoundException : ApplicationException
    {
        public LoggerNotFoundException()
            : base()
        {
        }

        public LoggerNotFoundException(string message)
            : base(message)
        {
        }
    }
}
