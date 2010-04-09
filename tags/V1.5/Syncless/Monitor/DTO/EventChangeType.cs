using System;

namespace Syncless.Monitor.DTO
{
    /// <summary>
    /// This enum specifies the type of event change.
    /// </summary>
    public enum EventChangeType
    {
        /// <summary>
        /// Creating a file.
        /// </summary>
        CREATING,/// <summary>
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
