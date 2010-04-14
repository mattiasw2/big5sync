/*
 * 
 * Author: Koh Cher Guan
 * 
 */

using System.IO;

namespace Syncless.Monitor.DTO
{
    /// <summary>
    /// A Data Transfer Object to pass information regarding a file event
    /// </summary>
    public class FileChangeEvent
    {
        private FileInfo _oldPath;
        /// <summary>
        /// Gets or sets a value for the affected file path.
        /// </summary>
        public FileInfo OldPath
        {
            get { return _oldPath; }
            set { _oldPath = value; }
        }

        private FileInfo _newPath;
        /// <summary>
        /// Gets or sets a value for the new file path. Only applicable to rename event.
        /// </summary>
        public FileInfo NewPath
        {
            get { return _newPath; }
            set { _newPath = value; }
        }

        private EventChangeType _event;
        /// <summary>
        /// Gets or sets a value for the event change type.
        /// </summary>
        public EventChangeType Event
        {
            get { return _event; }
            set { _event = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Monitor.DTO.FileChangeEvent" /> class for non-rename event, given the specified file path and the event change type.
        /// </summary>
        /// <param name="oldPath">A <see cref="System.IO.FileInfo" /> class specifying the affected path.</param>
        /// <param name="e">A <see cref="Syncless.Monitor.DTO.EventChangeType" /> enum specifying the event change type.</param>
        public FileChangeEvent(FileInfo oldPath, EventChangeType e)
        {
            this._oldPath = oldPath;
            this._newPath = null;
            this._event = e;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Monitor.DTO.FileChangeEvent" /> class for rename event only, given the specified old file path and new file path.
        /// </summary>
        /// <param name="oldPath">A <see cref="System.IO.FileInfo" /> class specifying the old path.</param>
        /// <param name="newPath">A <see cref="System.IO.FileInfo" /> class specifying the new path.</param>
        public FileChangeEvent(FileInfo oldPath, FileInfo newPath)
        {
            this._oldPath = oldPath;
            this._newPath = newPath;
            this._event = EventChangeType.RENAMED;
        }
    }
}
