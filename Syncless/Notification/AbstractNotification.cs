using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    public abstract class AbstractNotification
    {
        private string _name;
        private NotificationCode _notificationCode;        
        private Guid _notificationid;
        /// <summary>
        /// The notification id in string.
        /// </summary>
        public string Notificationid
        {
            get { return _notificationid.ToString(); }
        }
        /// <summary>
        /// Notification Code , the Type of notification.
        /// Should be implemented by Sub-classes for the Queue Observer.
        /// </summary>
        public NotificationCode NotificationCode
        {
            get { return _notificationCode; }
            set { _notificationCode = value; }
        }
        /// <summary>
        /// The name of the Notification.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public AbstractNotification(string name, NotificationCode notificationCode)
        {
            this._name = name;
            this._notificationCode = notificationCode;
            this._notificationid = Guid.NewGuid();
        }
        /// <summary>
        /// Override the Default equal to compare the guid.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            AbstractNotification noti = obj as AbstractNotification;
            if (noti == null)
            {
                return false;
            }
            return noti.Notificationid.Equals(this._notificationid);
        }
        /// <summary>
        /// Override the default GetHashCode to get the Hash code of GUID.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this._notificationid.GetHashCode();
        }
    }
}
