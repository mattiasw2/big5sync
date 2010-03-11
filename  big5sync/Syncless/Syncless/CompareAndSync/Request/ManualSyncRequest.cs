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
        private string[] _unavailablePaths;

        public ManualSyncRequest(string[] paths, string[] unavailablePaths, List<Filter> filters,  SyncConfig syncConfig)
            : base(paths, filters)
        {
            _syncConfig = syncConfig;
            _unavailablePaths = unavailablePaths;
        }

        public SyncConfig Config
        {
            get { return _syncConfig; }
        }

        public string[] UnavailablePaths
        {
            get { return _unavailablePaths; }
        }
    }
}
