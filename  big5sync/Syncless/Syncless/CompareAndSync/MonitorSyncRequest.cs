using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public class MonitorSyncRequest
    {
        private MonitorPathPair _oldPath, _newPath = null;
        private List<MonitorPathPair> _dest;
        private FileChangeType _changeType;
        private bool _isFolder;

        public MonitorSyncRequest(MonitorPathPair oldPath, List<MonitorPathPair> dest, FileChangeType changeType, bool isFolder)
        {
            _oldPath = oldPath;
            _dest = dest;
            _changeType = changeType;
            _isFolder = isFolder;
        }

        public MonitorSyncRequest(MonitorPathPair oldPath, MonitorPathPair newPath, List<MonitorPathPair> dest, FileChangeType changeType, bool isFolder) :
            this(oldPath, dest, changeType,isFolder)
        {
            _newPath = newPath;
        }

        public MonitorPathPair OldPath
        {
            get { return _oldPath; }
        }

        public MonitorPathPair NewPath
        {
            get { return _newPath; }
        }

        public List<MonitorPathPair> Dest
        {
            get { return _dest; }
        }

        public FileChangeType ChangeType
        {
            get { return _changeType; }
        }

        public bool IsFolder
        {
            get { return _isFolder; }
        }

    }
}
