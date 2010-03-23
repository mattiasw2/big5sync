using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification.UINotification
{
    public class NewProfileNotification : AbstractNotification
    {
        public NewProfileNotification()
            : base("New profile notification", Syncless.Notification.NotificationCode.NEW_PROFILE_NOTIFICATION)
        {

        }
    }
}
