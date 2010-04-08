using System.Collections.Generic;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    public class ManualCompareRequest : ManualRequest
    {
        public ManualCompareRequest(string[] paths, List<Filter> filters, SyncConfig syncConfig, string tagName)
            : base(paths, filters, syncConfig, tagName)
        {
        }
    }
}
