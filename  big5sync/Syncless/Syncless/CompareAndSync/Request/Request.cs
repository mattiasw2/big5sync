using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public abstract class Request
    {
        protected List<string> _paths;

        public List<string> Paths
        {
            get { return _paths; }
        }

    }
}
