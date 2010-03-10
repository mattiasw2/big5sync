using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    public class ManualSyncRequest : ManualRequest
    {
        public ManualSyncRequest(string[] paths, List<Filter> filters)
            : base(paths, filters)
        {
        }
    }
}
