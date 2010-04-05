using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;

namespace Syncless.Notification
{
    public class SyncCompleteNotification : AbstractNotification
    {
        private string _tagName;
        private RootCompareObject _rco;
        public SyncCompleteNotification(string tagName,RootCompareObject rco)
            : base("Sync Complete Notification", Syncless.Notification.NotificationCode.SYNC_COMPLETE_NOTIFICATION)
        {
            _rco = rco;
            _tagName = tagName;
        }
        

        public string TagName
        {
            get { return _tagName; }
        }

        public RootCompareObject CompareObject
        {
            get { return _rco; }
        }
    }
}
