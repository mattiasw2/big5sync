using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging;

namespace Syncless.Notification
{
    public class UnMonitorPathNotification : AbstractNotification
    {
        private Tag _targetTag;

        public Tag TargetTag
        {
            get { return _targetTag; }
            set { _targetTag = value; }
        }
        private TaggedPath _targetPath;

        public TaggedPath TargetPath
        {
            get { return _targetPath; }
            set { _targetPath = value; }
        }

        public UnMonitorPathNotification(Tag tag, TaggedPath path):base("UnMonitor Path Notification" , Syncless.Notification.NotificationCode.UNMONITOR_PATH_NOTIFICATION)
        {
            this._targetTag = tag;
            this._targetPath = path;
        }
    }
}
