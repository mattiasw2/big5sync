using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging;

namespace Syncless.Notification.SLLNotification
{
    public class MonitorPathNotification : AbstractNotification
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

        public MonitorPathNotification(Tag tag, TaggedPath path):base("Monitor Path Notification" , Syncless.Notification.NotificationCode.MONITOR_PATH_NOTIFICATION)
        {
            this._targetTag = tag;
            this._targetPath = path;
        }
    }
}
