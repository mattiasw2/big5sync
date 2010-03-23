using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification.SLLNotification
{
    public class MonitorTagNotification :AbstractNotification
    {
        private string _tagname;

        public string Tagname
        {
            get { return _tagname; }
            set { _tagname = value; }
        }

        public MonitorTagNotification(string tagname)
            : base("Monitor Tag Notification", Syncless.Notification.NotificationCode.MONITOR_TAG_NOTIFICATION)
        {
            _tagname = tagname;
        }
    }
}
