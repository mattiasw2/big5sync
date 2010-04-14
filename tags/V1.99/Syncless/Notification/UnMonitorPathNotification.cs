/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using Syncless.Tagging;

namespace Syncless.Notification
{
    /// <summary>
    /// The notification for unmonitor path.
    /// </summary>
    public class UnMonitorPathNotification : AbstractNotification
    {
        /// <summary>
        /// Get and Set the <see cref="Tag"/>
        /// </summary>
        public Tag TargetTag { get; set; }
        
        private TaggedPath _targetPath;
        /// <summary>
        /// Get and Set the <see cref="TargetPath"/>
        /// </summary>
        public TaggedPath TargetPath
        {
            get { return _targetPath; }
            set { _targetPath = value; }
        }
        /// <summary>
        /// Initialize UnMonitorPathNotification
        /// </summary>
        /// <param name="tag">the related <see cref="Tag"/></param>
        /// <param name="path">the related <see cref="TaggedPath"/></param>
        public UnMonitorPathNotification(Tag tag, TaggedPath path):base("UnMonitor Path Notification" , NotificationCode.UnmonitorPathNotification)
        {
            TargetTag = tag;
            _targetPath = path;
        }
    }
}
