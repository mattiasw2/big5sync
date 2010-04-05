using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    public class AutoSyncCompleteNotification : AbstractNotification
    {
        public string Path
        {
            get;
            set;
        }
        public AutoSyncCompleteNotification(string path)
            : base("Auto Sync Complete Notification", Notification.NotificationCode.AutoSyncCompleteNotification)
        {
            Path = path;
        }
    }
}
