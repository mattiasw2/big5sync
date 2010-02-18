using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class MonitorSyncRequest
    {
        private string _tagName, _oldPath, _newPath = null;
        private List<string> _dest;
        private FileChangeType _changeType;

        public MonitorSyncRequest(string tagName, string oldPath, List<string> dest, FileChangeType changeType)
        {
            _tagName = tagName;
            _oldPath = oldPath;
            _dest = dest;
            _changeType = changeType;
        }

        public MonitorSyncRequest(string tagName, string oldPath, string newPath, List<string> dest, FileChangeType changeType) :
            this(tagName, oldPath, dest, changeType)
        {
            _newPath = newPath;
        }
    }
}
