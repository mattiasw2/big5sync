namespace Syncless.Notification.UINotification
{
    /// <summary>
    /// CancelSyncNotification class encloses properties for a notification for cancelling a sync request.
    /// </summary>
    public class CancelSyncNotification : AbstractNotification
    {
        private readonly string _tagName;
        private readonly bool _isCancellable;

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
        public bool IsCancellable
        {
            get { return _isCancellable; }
        }

        /// <summary>
        /// Creates a new CancelSyncNotification object
        /// </summary>
        /// <param name="tagName">The tag name of the sync request</param>
        /// <param name="isCancellable">The boolean value that represents whether a sync request
        /// is cancellable</param>
        public CancelSyncNotification(string tagName, bool isCancellable)
            : base("Cancel Sync Notification", Notification.NotificationCode.CancelSyncNotification)
        {
            _tagName = tagName;
            _isCancellable = isCancellable;
        }
    }
}
