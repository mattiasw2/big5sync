using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class CompareRequest : Request
    {
        public CompareRequest(List<string> paths)
        {
            base._paths = paths;
        }
    }
}
