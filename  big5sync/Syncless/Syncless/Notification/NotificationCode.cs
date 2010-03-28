using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    public enum NotificationCode
    {
        SYNC_START_NOTIFICATION,SYNC_COMPLETE_NOTIFICATION,COMPARE_COMPLETE_NOTIFICATION,NEW_PROFILE_NOTIFICATION,
        NOTHING_TO_SYNC_NOTIFICATION,Tagged_Folder_Deleted_Notification,

        MONITOR_PATH_NOTIFICATION,UNMONITOR_PATH_NOTIFICATION,ADD_TAG_NOTIFICATION,DEL_TAG_NOTIFICATION,MONITOR_TAG_NOTIFICATION,TAGGED_PATH_DELETED_NOTIFICATION,
        MessageNotification,
        CancelSyncNotification
    }
}
