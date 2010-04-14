/*
 * 
 * Author: Koh Cher Guan
 * 
 */

using System.IO;

namespace Syncless.Monitor.DTO
{
    /// <summary>
    /// A Data Transfer Object to pass information regarding a folder event
    /// </summary>
    public class FolderChangeEvent
    {
        private DirectoryInfo _oldPath;
        /// <summary>
        /// Gets or sets a value for the affected directory path.
        /// </summary>
        public DirectoryInfo OldPath
        {
            get { return _oldPath; }
            set { _oldPath = value; }
        }

        private DirectoryInfo _newPath;
        /// <summary>
        /// Gets or sets a value for the new directory path. Only applicable to rename event.
        /// </summary>
        public DirectoryInfo NewPath
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
        /// Initializes a new instance of the <see cref="Syncless.Monitor.DTO.FolderChangeEvent" /> class for non-rename event, given the specified directory path and the event change type.
        /// </summary>
        /// <param name="oldPath">A <see cref="System.IO.DirectoryInfo" /> class specifying the affected path.</param>
        /// <param name="e">A <see cref="Syncless.Monitor.DTO.EventChangeType" /> enum specifying the event change type.</param>
        public FolderChangeEvent(DirectoryInfo oldPath, EventChangeType e)
        {
            this._oldPath = oldPath;
            this._newPath = null;
            this._event = e;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Monitor.DTO.FolderChangeEvent" /> class for rename event only, given the specified old directory path and new directory path.
        /// </summary>
        /// <param name="oldPath">A <see cref="System.IO.DirectoryInfo" /> class specifying the old path.</param>
        /// <param name="newPath">A <see cref="System.IO.DirectoryInfo" /> class specifying the new path.</param>
        public FolderChangeEvent(DirectoryInfo oldPath, DirectoryInfo newPath)
        {
            this._oldPath = oldPath;
            this._newPath = newPath;
            this._event = EventChangeType.RENAMED;
        }
    }
}
