using System;

namespace Syncless.Logging
{
    public enum LogEventType
    {
        SYNC_STARTED,
        SYNC_STOPPED,
        APPEVENT_DRIVE_ADDED,
        APPEVENT_DRIVE_RENAMED,
        APPEVENT_PROFILE_LOAD_FAILED,
        APPEVENT_TAG_CREATED,
        APPEVENT_TAG_DELETED,
        APPEVENT_TAG_CONFIG_UPDATED,
        APPEVENT_FOLDER_TAGGED,
        APPEVENT_FOLDER_UNTAGGED,
        FSCHANGE_CREATED,
        FSCHANGE_MODIFIED,
        FSCHANGE_DELETED,
        FSCHANGE_RENAMED,
        FSCHANGE_ARCHIVED,
        FSCHANGE_CONFLICT,
        FSCHANGE_ERROR,
        UNKNOWN
    }
}
