using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification.UINotification
{
    public class CompareCompleteNotification : AbstractNotification
    {
        public CompareCompleteNotification()
            : base("Compare Complete Notification", Syncless.Notification.NotificationCode.CompareCompleteNotification)
        {
        }
    }
}
