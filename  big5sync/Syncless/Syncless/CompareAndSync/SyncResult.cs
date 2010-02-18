using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class SyncResult : Result
    {
        bool _success;

        public SyncResult(FileChangeType changeType, string from, bool success)
        {
            base.ChangeType = changeType;
            base.From = from;
            _success = success;
        }

        public SyncResult(FileChangeType changeType, string from, string to, bool success) :
            this (changeType, from, success)
        {
            base.To = to;
        }

        //TODO: More constructors/properties/attributes that take in Log, etc.
    }
}
