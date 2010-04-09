using Syncless.Tagging;

namespace Syncless.Notification
{
    /// <summary>
    /// The Notification to the User Interface that a Sync Request have been accepted and started.
    /// </summary>
    public class SyncStartNotification : AbstractNotification
    {
        /// <summary>
        /// The <see cref="SyncProgress"/> for the Sync Request that is started
        /// </summary>
        public SyncProgress Progress { get; set; }
        /// <summary>
        /// Name of the <see cref="Tag"/> of the Synchronization
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// Initialize a Sync Start Notification
        /// </summary>
        /// <param name="tagName">name of the tag</param>
        public SyncStartNotification(string tagName)
            : base("Sync Start Notification", NotificationCode.SyncStartNotification)
        {
            Progress = new SyncProgress(tagName);
            
            TagName = tagName;
        }

    }
}
