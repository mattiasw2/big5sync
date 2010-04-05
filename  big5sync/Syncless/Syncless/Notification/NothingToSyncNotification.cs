using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    public class NothingToSyncNotification : AbstractNotification
    {
        private string _tagName;

        public string TagName
        {
            get { return _tagName; }
            set { _tagName = value; }
        }

        public NothingToSyncNotification(string tagName):base("Nothing to Sync" , Syncless.Notification.NotificationCode.NOTHING_TO_SYNC_NOTIFICATION)
        {
            _tagName = tagName;

        }
    }
}
