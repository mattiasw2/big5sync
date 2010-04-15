/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
namespace Syncless.Notification
{
    /// <summary>
    /// Notification for a Tagged Folder Deleted
    /// </summary>
    public class TaggedFolderDeletedNotification : AbstractNotification
    {
        /// <summary>
        /// Get and Set the Path of the Folder
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Get and Set the Name of the tag associated with the Path
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// Initalize the TaggedFolderDeletedNotification
        /// </summary>
        /// <param name="path">the path of folder</param>
        /// <param name="tagName">the name of the tag</param>
        public TaggedFolderDeletedNotification(string path,string tagName)
            : base("Tagged Folder Delete Notification", NotificationCode.TaggedFolderDeletedNotification)
        {
            Path = path;
            TagName = tagName;
        }
    }
}
