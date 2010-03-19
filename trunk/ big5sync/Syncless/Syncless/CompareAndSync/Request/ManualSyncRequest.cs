using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    public class ManualSyncRequest : ManualRequest
    {
        private string[] _unavailablePaths;

        public ManualSyncRequest(string[] paths, string[] unavailablePaths, List<Filter> filters,  SyncConfig syncConfig)
            : base(paths, filters, syncConfig)
        {
            _unavailablePaths = unavailablePaths;
        }

        public string[] UnavailablePaths
        {
            get { return _unavailablePaths; }
        }
    }
}
