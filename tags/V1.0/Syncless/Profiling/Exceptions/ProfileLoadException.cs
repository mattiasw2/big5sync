using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Profiling.Exceptions
{
    public class ProfileLoadException:Exception
    {
        public ProfileLoadException(Exception e)
            : base(e.Message,e)
        {
        }
        public ProfileLoadException()
        {

        }
    }
}
