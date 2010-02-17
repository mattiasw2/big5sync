using System;
using System.IO;

namespace Syncless.Monitor
{
    public class FolderChangeEvent
    {
        private DirectoryInfo _oldPath;
        public DirectoryInfo OldPath
        {
            get { return _oldPath; }
            set { _oldPath = value; }
        }

        private DirectoryInfo _newPath;
        public DirectoryInfo NewPath
        {
            get { return _newPath; }
            set { _newPath = value; }
        }

        private EventChangeType _event;
        public EventChangeType Event
        {
            get { return _event; }
            set { _event = value; }
        }

        public FolderChangeEvent(DirectoryInfo oldPath, EventChangeType e)
        {
            this._oldPath = oldPath;
            this._newPath = null;
            this._event = e;
        }

        public FolderChangeEvent(DirectoryInfo oldPath, DirectoryInfo newPath)
        {
            this._oldPath = oldPath;
            this._newPath = newPath;
            this._event = EventChangeType.RENAMED;
        }
    }
}
