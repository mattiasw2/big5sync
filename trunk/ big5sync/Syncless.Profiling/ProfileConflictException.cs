using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Profiling
{
    public class ProfileConflictException : Exception
    {
        public ProfileConflictException(string message):base(message)
        {

        }
    }
}
