namespace Syncless.CompareAndSync.Enum
{
    /// <summary>
    /// This enum specifies the type of <c>AutoSyncRequest</c>.
    /// </summary>
    public enum AutoSyncRequestType
    {
        /// <summary>
        /// A folder or file is created
        /// </summary>
        New,

        /// <summary>
        /// A file is updated
        /// </summary>
        Update,

        /// <summary>
        /// A file is deleted
        /// </summary>
        Delete,

        /// <summary>
        /// A folder or file is renamed
        /// </summary>
        Rename
    }
}
