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

        public bool Notify
        {
            get { return _notify; }
            set { _notify = value; }
        }

        private string _tagname;

        public string Tagname
        {
            get { return _tagname; }
            set { _tagname = value; }
        }
        public ManualSyncRequest(string[] paths, List<Filter> filters,  SyncConfig syncConfig,string tagname,bool notify)
            : base(paths, filters, syncConfig)
        {
            _tagname = tagname;
            _notify = notify;
        }
    }
}
