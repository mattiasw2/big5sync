using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification.UINotification
{
    public class CancelSyncNotification : AbstractNotification
    {
        private string _tagName;
        public string TagName
        {
            get { return _tagName; }
            set { _tagName = value; }

        }

        public CancelSyncNotification(string tagName)
            : base("Cancel Sync Notification", Notification.NotificationCode.CancelSyncNotification)
        {
            this._tagName = tagName;
        }
    }
}
