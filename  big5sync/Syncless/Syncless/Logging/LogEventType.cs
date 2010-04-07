using System;

namespace Syncless.Logging
{
    /// <summary>
    /// This enum specifies the type of log event.
    /// </summary>
    public enum LogEventType
    {
        /// <summary>
        /// Synchronization has started.
        /// </summary>
        SYNC_STARTED,
        /// <summary>
        /// Synchronization has ended.
        /// </summary>
        SYNC_STOPPED,
        /// <summary>
        /// New drive detected.
        /// </summary>
        APPEVENT_DRIVE_ADDED,
        /// <summary>
        /// Drive is renamed.
        /// </summary>
        APPEVENT_DRIVE_RENAMED,
        /// <summary>
        /// Fail to load profile.
        /// </summary>
        APPEVENT_PROFILE_LOAD_FAILED,
        /// <summary>
        /// A Tag is created.
        /// </summary>
        APPEVENT_TAG_CREATED,
        /// <summary>
        /// A Tag is deleted.
        /// </summary>
        APPEVENT_TAG_DELETED,
        /// <summary>
        /// The Tag Config is updated. 
        /// </summary>
        APPEVENT_TAG_CONFIG_UPDATED,
        /// <summary>
        /// A Folder is tagged.
        /// </summary>
        APPEVENT_FOLDER_TAGGED,
        /// <summary>
        /// A folder is untagged.
        /// </summary>
        APPEVENT_FOLDER_UNTAGGED,
        FSCHANGE_CREATED,
        /// <summary>
        /// A File is modified.
        /// </summary>
        FSCHANGE_MODIFIED,
        /// <summary>
        /// A File/Folder is deleted.
        /// </summary>
        FSCHANGE_DELETED,
        /// <summary>
        /// A file/Folder is renamed.
        /// </summary>
        FSCHANGE_RENAMED,
        /// <summary>
        /// A file/folder is archived.
        /// </summary>
        FSCHANGE_ARCHIVED,
        /// <summary>
        /// There is a conflict when synchronizing a file/folder.
        /// </summary>
        FSCHANGE_CONFLICT,
        /// <summary>
        /// Error occur while synchronizing.
        /// </summary>
        FSCHANGE_ERROR,
        /// <summary>
        /// An measure to prevent exception when the log file is tampered.
        /// Do not use this when logging.
        /// </summary>
        UNKNOWN
    }
}
