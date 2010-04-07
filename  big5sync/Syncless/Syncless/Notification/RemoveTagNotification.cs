using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging;

namespace Syncless.Notification
{
    public class RemoveTagNotification : AbstractNotification 
    {
        private Tag _tag;

        public Tag Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }
        public RemoveTagNotification(Tag tag)
            : base("Add Tag Notification", Syncless.Notification.NotificationCode.AddTagNotification)
        {
            this._tag = tag;
        }
    }
}
