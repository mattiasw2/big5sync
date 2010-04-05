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
        private string _tagName;

        public string TagName
        {
            get { return _tagName; }
            set { _tagName = value; }
        }

        public SyncStartNotification(string tagName)
            : base("Sync Start Notification", Syncless.Notification.NotificationCode.SYNC_START_NOTIFICATION)
        {
            _progress = new SyncProgress(tagName);
            
            _tagName = tagName;
        }

    }
}
