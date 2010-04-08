using System.Collections.Generic;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    public class ManualSyncRequest : ManualRequest
    {
        private readonly bool _notify;

        public ManualSyncRequest(string[] paths, List<Filter> filters, SyncConfig syncConfig, string tagName, bool notify)
            : base(paths, filters, syncConfig, tagName)
        {
            _notify = notify;
        }

        public bool Notify
        {
            get { return _notify; }
        }
    }
}
