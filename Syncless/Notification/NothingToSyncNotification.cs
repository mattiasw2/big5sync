namespace Syncless.Notification
{
    /// <summary>
    /// Notification to inform the UI that there is nothing to Synchronzing.
    /// </summary>
    public class NothingToSyncNotification : AbstractNotification
    {
        /// <summary>
        /// Get and Set the name of the Tag
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// Initialize the NothingToSyncNotification
        /// </summary>
        /// <param name="tagName"></param>
        public NothingToSyncNotification(string tagName):base("Nothing to Sync" , Syncless.Notification.NotificationCode.NothingToSyncNotification)
        {
            TagName = tagName;
        }
    }
}
