using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    public class ManualSyncRequest : ManualRequest
    {
        private SyncConfig _syncConfig;

        public ManualSyncRequest(string[] paths, string[] unavailablePaths, List<Filter> filters,  SyncConfig syncConfig)
            : base(paths, unavailablePaths, filters)
        {
            _syncConfig = syncConfig;
        }

        public SyncConfig Config
        {
            get { return _syncConfig; }
        }
    }
}
