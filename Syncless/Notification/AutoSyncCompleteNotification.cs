/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
namespace Syncless.Notification
{
    /// <summary>
    /// The notification for automatic synchronization completed.
    /// </summary>
    public class AutoSyncCompleteNotification : AbstractNotification
    {
        /// <summary>
        /// Gets or sets the path that is synchronized
        /// </summary>
        public string Path
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new AutoSyncCompleteNotification object
        /// </summary>
        /// <param name="path">The path that is synchronized</param>
        public AutoSyncCompleteNotification(string path)
            : base("Auto Sync Complete Notification", Notification.NotificationCode.AutoSyncCompleteNotification)
        {
            Path = path;
        }
    }
}
