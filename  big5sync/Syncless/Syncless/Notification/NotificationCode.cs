using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    /// <summary>
    /// NotificationCode enum provides the enumerations of notification type.
    /// </summary>
    public enum NotificationCode
    {
        SyncStartNotification,
        SyncCompleteNotification,
        CompareCompleteNotification,
        NewProfileNotification,
        NothingToSyncNotification,
        TaggedFolderDeletedNotification,
        MonitorPathNotification,
        UnmonitorPathNotification,
        AddTagNotification,
        DeleteTagNotification,
        MonitorTagNotification,
        TaggedPathDeletedNotification,
        MessageNotification,
        CancelSyncNotification,
        AutoSyncCompleteNotification,
        SaveNotification
    }
}
