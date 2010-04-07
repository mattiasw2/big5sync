using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Monitor.DTO
{
    /// <summary>
    /// A Data Transfer Object to pass information regarding a file system event
    /// </summary>
    public class FileSystemEvent
    {
        private string _path;
        /// <summary>
        /// Gets or sets a value for the affected path.
        /// </summary>
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        private string _oldPath;
        /// <summary>
        /// Gets or sets a value for the old path. Only applicable to rename event.
        /// </summary>
        public string OldPath
        {
            get { return _oldPath; }
            set { _oldPath = value; }
        }

        private string _watchPath;
        /// <summary>
        /// Gets or sets a value for the path being watched.
        /// </summary>
        public string WatchPath
        {
            get { return _watchPath; }
            set { _watchPath = value; }
        }

        private EventChangeType _eventType;
        /// <summary>
        /// Gets or sets a value for the event change type.
        /// </summary>
        public EventChangeType EventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }

        private FileSystemType _fileSystemType;
        /// <summary>
        /// Gets or sets a value for the file system change type.
        /// </summary>
        public FileSystemType FileSystemType
        {
            get { return _fileSystemType; }
            set { _fileSystemType = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Monitor.DTO.FileSystemEvent" /> class for non-rename event, given the specified path, the event change type and the file system change type.
        /// </summary>
        /// <param name="path">A <see cref="string" /> specifying the affected path.</param>
        /// <param name="eventType">A <see cref="Syncless.Monitor.DTO.EventChangeType" /> enum specifying the event change type.</param>
        /// <param name="fileSystemType">A <see cref="Syncless.Monitor.DTO.FileSystemChangeType" /> enum specifying the file system change type.</param>
        public FileSystemEvent(string path, EventChangeType eventType, FileSystemType fileSystemType)
        {
            this._path = path;
            this._oldPath = null;
            this._watchPath = null;
            this._eventType = eventType;
            this._fileSystemType = fileSystemType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Monitor.DTO.FileSystemEvent" /> class for rename event only, given the old path, the new path and the file system change type.
        /// </summary>
        /// <param name="oldPath">A <see cref="string" /> class specifying the old path.</param>
        /// <param name="newPath">A <see cref="string" /> class specifying the new path.</param>
        /// <param name="fileSystemType">A <see cref="Syncless.Monitor.DTO.FileSystemChangeType" /> enum specifying the file system change type.</param>
        public FileSystemEvent(string oldPath, string newPath, FileSystemType fileSystemType)
        {
            this._oldPath = oldPath;
            this._path = newPath;
            this._watchPath = null;
            this._eventType = EventChangeType.RENAMED;
            this._fileSystemType = fileSystemType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Monitor.DTO.FileSystemEvent" /> class for delete event with unknown file system type only, given the affected path and the path being monitored.
        /// </summary>
        /// <param name="path">A <see cref="string" /> specifying the affected path.</param>
        /// <param name="watchPath">A <see cref="string" /> specifying the path being monitored.</param>
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
