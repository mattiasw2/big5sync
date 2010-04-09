using Syncless.Tagging;

namespace Syncless.Notification
{
    /// <summary>
    /// The Notification for adding a Tag.
    /// </summary>
    public class AddTagNotification : AbstractNotification 
    {
        /// <summary>
        /// Get and Set the related Tag.
        /// </summary>
        public Tag Tag { get; set; }
        /// <summary>
        /// Initialize the AddTagNotification
        /// </summary>
        /// <param name="tag"></param>
        public AddTagNotification(Tag tag)
            : base("Add Tag Notification", NotificationCode.AddTagNotification)
        {
            Tag = tag;
        }
    }
}
