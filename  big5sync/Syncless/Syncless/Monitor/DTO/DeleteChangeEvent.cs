using System;
using System.IO;

namespace Syncless.Monitor.DTO
{
    public class DeleteChangeEvent
    {
        private DirectoryInfo _path;
        public DirectoryInfo Path
        {
            get { return _path; }
            set { _path = value; }
        }

        private const EventChangeType _event = EventChangeType.DELETED;
        public EventChangeType Event
        {
            get { return _event; }
        }

        private DirectoryInfo _watcherPath;
        public DirectoryInfo WatcherPath
        {
            get { return _watcherPath; }
            set { _watcherPath = value; }
        }

        public DeleteChangeEvent(DirectoryInfo path, DirectoryInfo watcherPath)
        {
            this._path = path;
            this._watcherPath = watcherPath;
        }
    }
}
