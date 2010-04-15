/*
 * 
 * Author: Soh Yuan Chin
 * 
 */

namespace Syncless.CompareAndSync.Enum
{
    /// <summary>
    /// This enum specifies the change of a file or folder with respect to its own metadata.
    /// </summary>
    public enum MetaChangeType
    {
        /// <summary>
        /// A folder or file is deleted
        /// </summary>
        Delete,

        /// <summary>
        /// A folder or file is created
        /// </summary>
        New,

        /// <summary>
        /// A folder or file is unchanged
        /// </summary>
        NoChange,

        /// <summary>
        /// A folder or file is renamed
        /// </summary>
        Rename,

        /// <summary>
        /// A file is updated
        /// </summary>
        Update //File
    }
}
