using Syncless.Tagging;

namespace Syncless.Notification
{
    /// <summary>
    /// The notification for remove tag
    /// </summary>
    public class RemoveTagNotification : AbstractNotification 
    {
        /// <summary>
        /// Get and Set the related Tag
        /// </summary>
        public Tag Tag { get; set; }
        /// <summary>
        /// Initialize the RemoveTagNotification
        /// </summary>
        /// <param name="tag"></param>
        public RemoveTagNotification(Tag tag)
            : base("Add Tag Notification", NotificationCode.AddTagNotification)
        {
            Tag = tag;
        }
    }
}
