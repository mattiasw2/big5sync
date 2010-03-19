using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    public class ManualCompareRequest : ManualRequest
    {
        public ManualCompareRequest(string[] paths, List<Filter> filters, SyncConfig syncConfig)
            : base(paths, filters, syncConfig)
        {
        }
    }
}
