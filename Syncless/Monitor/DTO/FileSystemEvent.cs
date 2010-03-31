using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Monitor.DTO
{
    public class FileSystemEvent
    {
        private string _path;
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        private string _oldPath;
        public string OldPath
        {
            get { return _oldPath; }
            set { _oldPath = value; }
        }

        private string _watchPath;
        public string WatchPath
        {
            get { return _watchPath; }
            set { _watchPath = value; }
        }

        private EventChangeType _eventType;
        public EventChangeType EventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }

        private FileSystemType _fileSystemType;
        public FileSystemType FileSystemType
        {
            get { return _fileSystemType; }
            set { _fileSystemType = value; }
        }

        public FileSystemEvent(string path, EventChangeType eventType, FileSystemType fileSystemType)
        {
            this._path = path;
            this._oldPath = null;
            this._watchPath = null;
            this._eventType = eventType;
            this._fileSystemType = fileSystemType;
        }

        public FileSystemEvent(string oldPath, string newPath, FileSystemType fileSystemType)
        {
            this._oldPath = oldPath;
            this._path = newPath;
            this._watchPath = null;
            this._eventType = EventChangeType.RENAMED;
            this._fileSystemType = fileSystemType;
        }

        public FileSystemEvent(string path, string watchPath)
        {
            this._path = path;
            this._oldPath = null;
            this._watchPath = watchPath;
            this._eventType = EventChangeType.DELETED;
            this._fileSystemType = FileSystemType.UNKNOWN;
        }
    }
}
