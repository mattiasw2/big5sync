namespace Syncless.Tagging
{
    /// <summary>
    /// LogMessage provides formatted string values for logging events in Tagging component
    /// </summary>
    internal static class LogMessage
    {
        /// <summary>
        /// The string value for tag creation event log given by "Tag \'{0}\' created."
        /// </summary>
        /// <remarks>{0} will be replaced by a tag name property when used.</remarks>
        internal static string TAG_CREATED = "Tag \'{0}\' created.";

        /// <summary>
        /// The string value for tag rename event log given by "Tag \'{0}\' renamed to \'{1}\'." 
        /// </summary>
        /// <remarks>{0} will be replaced by the old tag name property and {1} replaced by the new tag 
        /// name property when used.</remarks>
        internal static string TAG_RENAMED = "Tag \'{0}\' renamed to \'{1}\'.";

        /// <summary>
        /// The string value for tag removal event log given by "Tag \'{0}\' removed." 
        /// </summary>
        /// <remarks>{0} will be replaced by a tag name property when used.</remarks>
        internal static string TAG_REMOVED = "Tag \'{0}\' removed.";

        /// <summary>
        /// The string value for folder tagging event log given by "Folder \'{0}\' tagged to tag \'{1}\'."
        /// </summary>
        /// <remarks>{0} will be replaced by a folder path property and {1} replaced by a tag name property when
        /// used.</remarks>
        internal static string FOLDER_TAGGED = "Folder \'{0}\' tagged to tag \'{1}\'.";

        /// <summary>
        /// The string value for folder untagging event log given by "Folder \'{0}\' untagged from 
        /// tag \'{1}\'."
        /// </summary>
        /// <remarks>{0} will be replaced by a folder path property and {1} replaced by a tag name 
        /// property when used.</remarks>
        internal static string FOLDER_UNTAGGED = "Folder \'{0}\' untagged from tag \'{1}\'.";

        /// <summary>
        /// The string value for unsuccessful folder untagging event log given by "Unable to untag 
        /// folder \'{0}\' from tag \'{1}\'."
        /// </summary>
        /// <remarks>{0} will be replaced by a folder path property and {1} replaced by a tag name 
        /// property when used.</remarks>
        internal static string FOLDER_NOT_UNTAGGED = "Unable to untag folder \'{0}\' from tag \'{1}\'.";

        /// <summary>
        /// The string value for folder rename event log given by "Folder \'{0}\' renamed to \'{1}\'."
        /// </summary>
        /// <remarks>{0} will be replaced by the old folder name property and {1} replaced by the new folder 
        /// name property when used.</remarks>
        internal static string FOLDER_RENAMED = "Folder \'{0}\' renamed to \'{1}\'.";

        /// <summary>
        /// The string value for tag not found event log given by "Tag \'{0}\' cannot be found."
        /// </summary>
        /// <remarks>{0} will be replaced by a tag name property when used.</remarks>
        internal static string TAG_NOT_FOUND = "Tag \'{0}\' cannot be found.";

        /// <summary>
        /// The string value for tag already exists event log given by "Tag \'{0}\' already exists."
        /// </summary>
        /// <remarks>{0} will be replaced by a tag name property when used.</remarks>
        internal static string TAG_ALREADY_EXISTS = "Tag \'{0}\' already exists.";

        /// <summary>
        /// The string value for path not found in tag event log given by "Path \'{0}\' not found in 
        /// tag \'{1}\'."
        /// </summary>
        /// <remarks>{0} will be replaced by a path name property and {1} replaced by a tag 
        /// name property when used.</remarks>
        internal static string PATH_NOT_FOUND_IN_TAG = "Path \'{0}\' not found in tag \'{1}\'.";

        /// <summary>
        /// The string value for path already exists in tag event log given by "Path \'{0}\' already 
        /// exists in tag \'{1}\'."
        /// </summary>
        /// <remarks>{0} will be replaced by a path name property and {1} replaced by a tag name 
        /// property when used.</remarks>
        internal static string PATH_ALREADY_EXISTS_IN_TAG = "Path \'{0}\' already exists in tag \'{1}\'.";

        /// <summary>
        /// The string value for tag removal event log given by "Attempt to tag parent/sub directory \'{0}\' 
        /// of existing directory."
        /// </summary>
        /// <remarks>{0} will be replaced by a path name property when used.</remarks>
        internal static string RECURSIVE_DIRECTORY = "Attempt to tag parent/sub directory \'{0}\' of existing directory.";

        /// <summary>
        /// The string value for filter adding event log given by "Filter added to tag \'{0}\'."
        /// </summary>
        /// <remarks>{0} will be replaced by a tag name property when used.</remarks>
        internal static string FILTER_ADDED = "Filter added to tag \'{0}\'.";

        /// <summary>
        /// The string value for filter removal event log given by "Filter removed from tag \'{0}\'."
        /// </summary>
        /// <remarks>{0} will be replaced by a tag name property when used.</remarks>
        internal static string FILTER_REMOVED = "Filter removed from tag \'{0}\'.";

        /// <summary>
        /// The string value for unsuccessful filter adding event log given by "Filter cannot be added 
        /// to tag \'{0}\'."
        /// </summary>
        /// <remarks>{0} will be replaced by a tag name property when used.</remarks>
        internal static string FILTER_NOT_ADDED = "Filter cannot be added to tag \'{0}\'.";

        /// <summary>
        /// The string value for unsuccessful filter removal event log given by "Filter cannot be removed 
        /// from tag \'{0}\'."
        /// </summary>
        /// <remarks>{0} will be replaced by a tag name property when used.</remarks>
        internal static string FILTER_NOT_REMOVED = "Filter cannot be removed from tag \'{0}\'.";

        /// <summary>
        /// The string value for path not found event log given by "Path \'{0}\' does not exist."
        /// </summary>
        /// <remarks>{0} will be replaced by a path name property when used.</remarks>
        internal static string PATH_NOT_FOUND = "Path \'{0}\' does not exist.";

        /// <summary>
        /// The string value for path already exists event log given by "Path \'{0}\' already exists."
        /// </summary>
        /// <remarks>{0} will be replaced by a path name property when used.</remarks>
        internal static string PATH_ALREADY_EXISTS = "Path \'{0}\' already exists.";
    }
}
