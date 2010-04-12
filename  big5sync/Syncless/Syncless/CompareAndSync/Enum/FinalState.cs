namespace Syncless.CompareAndSync.Enum
{

    /// <summary>
    /// This enum specifies the final state of a file after a manual synchronization.
    /// </summary>
    public enum FinalState
    {
        /// <summary>
        /// File is deleted
        /// </summary>
        Deleted,

        /// <summary>
        /// File is updated
        /// </summary>
        Updated,

        /// <summary>
        /// File is created
        /// </summary>
        Created,

        /// <summary>
        /// File is renamed
        /// </summary>
        Renamed,

        /// <summary>
        /// File is created and renamed
        /// </summary>
        CreatedRenamed,

        /// <summary>
        /// File is unchanged
        /// </summary>
        Unchanged,

        /// <summary>
        /// Error handling file
        /// </summary>
        Error
    }
}
