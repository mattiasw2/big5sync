using System;

namespace Syncless.Monitor
{
    public enum EventChangeType
    {
        /// <summary>
        /// A File or folder is created.
        /// </summary>
        CREATED,
        /// <summary>
        /// A File or Folder is deleted.
        /// </summary>
        DELETED,
        /// <summary>
        /// A File is modified.
        /// </summary>
        MODIFIED,
        /// <summary>
        /// A File or Folder is renamed.
        /// </summary>
        RENAMED
    }
}
