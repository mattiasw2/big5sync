using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class MonitorSyncRequest
    {
        private string _oldPath, _newPath = null;
        private List<string> _dest;
        private FileChangeType _changeType;
        private bool _isFolder;
        public MonitorSyncRequest(string oldPath, List<string> dest, FileChangeType changeType,bool isFolder)
        {
            _oldPath = oldPath;
            _dest = dest;
            _changeType = changeType;
            _isFolder = isFolder;
        }

        public MonitorSyncRequest(string oldPath, string newPath, List<string> dest, FileChangeType changeType,bool isFolder) :
            this(oldPath, dest, changeType,isFolder)
        {
            _newPath = newPath;
        }

        public string OldPath
        {
            get { return _oldPath; }
        }

        public string NewPath
        {
            get { return _newPath; }
        }

        public List<string> Dest
        {
            get { return _dest; }
        }

        public FileChangeType ChangeType
        {
            get { return _changeType; }
        }

    }
}
