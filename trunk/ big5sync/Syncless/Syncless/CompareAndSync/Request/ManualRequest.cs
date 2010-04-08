using System.Collections.Generic;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    public abstract class ManualRequest : Request
    {
        private readonly string[] _paths;
        private readonly List<Filter> _filters;
        private readonly SyncConfig _syncConfig;
        private readonly string _tagName;

        protected ManualRequest(string[] paths, List<Filter> filters, SyncConfig syncConfig, string tagName)
        {
            _paths = paths;
            _filters = filters;
            _syncConfig = syncConfig;
            _tagName = tagName;
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

        public string TagName
        {
            get { return _tagName; }
        }

    }
}
