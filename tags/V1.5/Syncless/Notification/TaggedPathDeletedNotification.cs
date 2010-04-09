using System.Collections.Generic;

namespace Syncless.Notification
{
    /// <summary>
    /// Notification of a TaggedPath is deleted.
    /// </summary>
    public class TaggedPathDeletedNotification : AbstractNotification
    {
        /// <summary>
        /// The list of deleted paths
        /// </summary>
        public List<string> DeletedPaths { get; set; }
        /// <summary>
        /// Initialize the TaggedPathDeletedNotification
        /// </summary>
        /// <param name="deletedPaths"></param>
        public TaggedPathDeletedNotification(List<string> deletedPaths)
            : base("Tagged Path Deleted Notification", NotificationCode.TaggedPathDeletedNotification)
        {
            DeletedPaths = deletedPaths;
        }
    }
}
