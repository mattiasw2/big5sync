/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
namespace Syncless.Notification
{
    /// <summary>
    /// NotificationCode enum provides the enumerations of notification type.
    /// </summary>
    public enum NotificationCode
    {
        /// <summary>
        /// The notification for sync started
        /// </summary>
        SyncStartNotification,
        /// <summary>
        /// The notification for sync completed
        /// </summary>
        SyncCompleteNotification,
        /// <summary>
        /// The notification for compare completed
        /// </summary>
        CompareCompleteNotification,
        /// <summary>
        /// The notification for new profile
        /// </summary>
        NewProfileNotification,
        /// <summary>
        /// The notification for nothing to sync
        /// </summary>
        NothingToSyncNotification,
        /// <summary>
        /// The notification for tagged folder deleted
        /// </summary>
        TaggedFolderDeletedNotification,
        /// <summary>
        /// The notification for monitor path request
        /// </summary>
        MonitorPathNotification,
        /// <summary>
        /// The notification for unmonitor path request
        /// </summary>
        UnmonitorPathNotification,
        /// <summary>
        /// The notification for tag added
        /// </summary>
        AddTagNotification,
        /// <summary>
        /// The notification for tag deleted
        /// </summary>
        DeleteTagNotification,
        /// <summary>
        /// The notification for monitor tag request
        /// </summary>
        MonitorTagNotification,
        /// <summary>
        /// The notification for tagged path deleted
        /// </summary>
        TaggedPathDeletedNotification,
        /// <summary>
        /// The notification for sending a message
        /// </summary>
        MessageNotification,
        /// <summary>
        /// The notification for cancel sync request
        /// </summary>
        CancelSyncNotification,
        /// <summary>
        /// The notification for auto sync completed
        /// </summary>
        AutoSyncCompleteNotification,
        /// <summary>
        /// The notification for saving xml files
        /// </summary>
        SaveNotification
    }
}
