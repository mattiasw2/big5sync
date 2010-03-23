using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Filters;

namespace Syncless.CompareAndSync.Request
{
    public class ManualSyncRequest : ManualRequest
    {
        private bool _notify;
        private string _tagName;

        public ManualSyncRequest(string[] paths, List<Filter> filters, SyncConfig syncConfig, string tagName, bool notify)
            : base(paths, filters, syncConfig)
        {
            _tagName = tagName;
            _notify = notify;
        }

        public bool Notify
        {
            get { return _notify; }
            set { _notify = value; }
        }

        public string TagName
        {
            get { return _tagName; }
            set { _tagName = value; }
        }
    }
}
