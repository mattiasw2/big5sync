using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
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
