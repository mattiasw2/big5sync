using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    public class SyncCompleteNotification : AbstractNotification
    {
        public SyncCompleteNotification()
            : base("Sync Complete Notification", Syncless.Notification.NotificationCode.SYNC_COMPLETE_NOTIFICATION)
        {

        }
    }
}
