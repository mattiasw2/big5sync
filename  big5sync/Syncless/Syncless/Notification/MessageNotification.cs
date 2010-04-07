using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    /// <summary>
    /// MessageNotification class encloses properties for a notification sending message.
    /// </summary>
    public class MessageNotification : AbstractNotification
    {
        /// <summary>
        /// Creates a new MessageNotification object
        /// </summary>
        /// <param name="message">The string value that represents the message to be sent</param>
        public MessageNotification(string message)
            : base(message, Notification.NotificationCode.MessageNotification)
        {
        }
    }
}
