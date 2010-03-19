using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    public abstract class ManualRequest : Request
    {
        private string[] _paths;
        private List<Filter> _filters;
        private SyncConfig _syncConfig;

        public ManualRequest(string[] paths, List<Filter> filters, SyncConfig syncConfig)
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
