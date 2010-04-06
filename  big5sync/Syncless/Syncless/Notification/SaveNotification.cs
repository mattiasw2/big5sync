using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    public class SaveNotification : AbstractNotification
    {
        public SaveNotification()
            : base("Save Notification", Notification.NotificationCode.SaveNotification)
        {

        }

        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return true; // return true as long as their type is the same.
            

        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
