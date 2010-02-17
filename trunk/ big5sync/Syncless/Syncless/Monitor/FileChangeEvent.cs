using System;
using System.IO;

namespace Syncless.Monitor
{
    public class FileChangeEvent
    {
        private FileInfo _oldPath;
        public FileInfo OldPath
        {
            get { return _oldPath; }
            set { _oldPath = value; }
        }

        private FileInfo _newPath;
        public FileInfo NewPath
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

        public FileChangeEvent(FileInfo oldPath, FileInfo newPath, EventChangeType e)
        {
            this._oldPath = oldPath;
            this._newPath = newPath;
            this._event = e;
        }
    }
}
