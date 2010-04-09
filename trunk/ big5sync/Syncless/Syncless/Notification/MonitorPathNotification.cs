using Syncless.Tagging;

namespace Syncless.Notification
{
    /// <summary>
    /// Notification to inform logic to monitor a path
    /// </summary>
    public class MonitorPathNotification : AbstractNotification
    {
        /// <summary>
        /// Get and Set the target <see cref="Tag"/>
        /// </summary>
        public Tag TargetTag { get; set; }
        /// <summary>
        /// Get and Set the target <see cref="TaggedPath"/>
        /// </summary>
        public TaggedPath TargetPath { get; set; }
        /// <summary>
        /// Initalize the MonitorPathNotificaiton
        /// </summary>
        /// <param name="tag">The target <see cref="Tag"/></param>
        /// <param name="path">The target <see cref="TaggedPath"/></param>
        public MonitorPathNotification(Tag tag, TaggedPath path):base("Monitor Path Notification" , Syncless.Notification.NotificationCode.MonitorPathNotification)
        {
            TargetTag = tag;
            TargetPath = path;
        }
    }
}
