using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification.SLLNotification
{
    public class MessageNotification :AbstractNotification 
    {
        public MessageNotification(string message)
            : base(message, "201")
        {
        }
    }
}
