using System;
using System.IO;

namespace Syncless.Monitor.DTO
{
    /// <summary>
    /// A Data Transfer Object to pass information regarding a delete event
    /// </summary>
    public class DeleteChangeEvent
    {
        private DirectoryInfo _path;
        /// <summary>
        /// Gets or sets a value for the directory path.
        /// </summary>
        public DirectoryInfo Path
        {
            get { return _path; }
            set { _path = value; }
        }

        private const EventChangeType _event = EventChangeType.DELETED;
        /// <summary>
        /// Gets a value for the event change type.
        /// </summary>
        public EventChangeType Event
        {
            get { return _event; }
        }

        private DirectoryInfo _watcherPath;
        /// <summary>
        /// Gets or sets a value for the directory path being watched.
        /// </summary>
        public DirectoryInfo WatcherPath
        {
            get { return _watcherPath; }
            set { _watcherPath = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Monitor.DTO.DeleteChangeEvent" /> class, given the specified directory path and the path being monitored.
        /// </summary>
        /// <param name="path">A <see cref="System.IO.DirectoryInfo" /> class specifying the deleted path.</param>
        /// <param name="watcherPath">A <see cref="System.IO.DirectoryInfo" /> class specifying the path being monitored.</param>
        public DeleteChangeEvent(DirectoryInfo path, DirectoryInfo watcherPath)
        {
            this._path = path;
            this._watcherPath = watcherPath;
        }
    }
}
