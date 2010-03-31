using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification.UINotification
{
    public class CancelSyncNotification : AbstractNotification
    {
        private readonly string _tagName;
        private readonly bool _isCancellable;

        public string TagName
        {
            get { return _tagName; }
        }

        public bool IsCancellable
        {
            get { return _isCancellable; }
        }

        public CancelSyncNotification(string tagName, bool isCancellable)
            : base("Cancel Sync Notification", Notification.NotificationCode.CancelSyncNotification)
        {
            _tagName = tagName;
            _isCancellable = isCancellable;
        }
    }
}
