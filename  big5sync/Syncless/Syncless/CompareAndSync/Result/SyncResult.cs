using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class SyncResult : Result
    {
        bool _success;

        public SyncResult(FileChangeType changeType, string from, bool success) :
            base(changeType, from)
        {
            _success = success;
        }

        public SyncResult(FileChangeType changeType, string from, string to, bool success) :
            base(changeType, from, to)
        {
            base.To = to;
        }

        public bool Success
        {
            get
            {
                return _success;
            }
        }

        //TODO: More constructors/properties/attributes that take in Log, etc.
    }
}
