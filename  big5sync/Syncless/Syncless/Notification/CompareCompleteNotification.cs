using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification.UINotification
{
    /// <summary>
    /// CompareCompleteNotification class encloses properties for a notification for completing a compare.
    /// </summary>
    public class CompareCompleteNotification : AbstractNotification
    {
        /// <summary>
        /// Creates a new CompareCompleteNotification object
        /// </summary>
        public CompareCompleteNotification()
            : base("Compare Complete Notification", Syncless.Notification.NotificationCode.CompareCompleteNotification)
        {
        }
    }
}
