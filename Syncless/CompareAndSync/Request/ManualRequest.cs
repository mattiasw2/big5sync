using System.Collections.Generic;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    public abstract class ManualRequest : Request
    {
        private readonly string[] _paths;
        private readonly List<Filter> _filters;
        private readonly SyncConfig _syncConfig;

        protected ManualRequest(string[] paths, List<Filter> filters, SyncConfig syncConfig)
        {
            _paths = paths;
            _filters = filters;
            _syncConfig = syncConfig;
        }

        public string[] Paths
        {
            get { return _paths; }
        }

        public List<Filter> Filters
        {
            get { return _filters; }
        }

        public SyncConfig Config
        {
            get { return _syncConfig; }
        }

    }
}
