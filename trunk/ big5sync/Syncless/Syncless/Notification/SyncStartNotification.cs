using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    /// <summary>
    /// The Notification to the User Interface that a Sync Request have been accepted and started.
    /// </summary>
    public class SyncStartNotification : AbstractNotification
    {
        private SyncProgress _progress;

        public SyncProgress Progress
        {
            get { return _progress; }
            set { _progress = value; }
        }

        public SyncStartNotification(int jobCount)
            : base("Sync Start Notification", Syncless.Notification.NotificationCode.SYNC_COMPLETE_NOTIFICATION)
        {
            _progress = new SyncProgress(jobCount);
        }

    }
}
