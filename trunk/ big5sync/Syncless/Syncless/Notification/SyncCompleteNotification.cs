using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.Tagging;

namespace Syncless.Notification
{
    /// <summary>
    /// The nofication for Sync Complete
    /// </summary>
    public class SyncCompleteNotification : AbstractNotification
    {
        /// <summary>
        /// Initialize the Sync Complete Notification
        /// </summary>
        /// <param name="tagName">name of the tag</param>
        /// <param name="rco">the root compare object</param>
        public SyncCompleteNotification(string tagName,RootCompareObject rco)
            : base("Sync Complete Notification", NotificationCode.SyncCompleteNotification)
        {
            CompareObject = rco;
            TagName = tagName;
        }

        /// <summary>
        /// Get the name of the <see cref="Tag"/>
        /// </summary>
        public string TagName { get; private set; }

        /// <summary>
        /// The RootCompare object containing the Sync Information
        /// </summary>
        public RootCompareObject CompareObject { get; private set; }
    }
}
