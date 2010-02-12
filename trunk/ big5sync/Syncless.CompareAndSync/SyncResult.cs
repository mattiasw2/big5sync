using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class SyncResult : Result
    {
        public SyncResult(FileChangeType changeType, string from, string to)
        {
            base.ChangeType = changeType;
            base.From = from;
            base.To = to;
        }
        //TODO: More constructors/properties/attributes that take in Log, etc.
    }
}
