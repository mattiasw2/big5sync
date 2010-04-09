namespace Syncless.Notification
{
    /// <summary>
    /// CancelSyncNotification class encloses properties for a notification for cancelling a sync request.
    /// </summary>
    public class CancelSyncNotification : AbstractNotification
    {
        private readonly string _tagName;
        private readonly bool _isCancel;

        /// <summary>
        /// Gets the tag name
        /// </summary>
        public string TagName
        {
            get { return _tagName; }
        }

        /// <summary>
        /// Gets the boolean value that represents whether a sync request is cancellable
        /// </summary>
        public bool IsCancel
        {
            get { return _isCancel; }
        }

        /// <summary>
        /// Creates a new CancelSyncNotification object
        /// </summary>
        /// <param name="tagName">The tag name of the sync request</param>
        /// <param name="isCancel">The boolean value that represents whether a sync request
        /// is canceled</param>
        public CancelSyncNotification(string tagName, bool isCancel)
            : base("Cancel Sync Notification", Notification.NotificationCode.CancelSyncNotification)
        {
            _tagName = tagName;
            _isCancel = isCancel;
        }
    }
}
