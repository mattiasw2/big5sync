namespace Syncless.CompareAndSync.Enum
{

    /// <summary>
    /// This enum specifies the final state of a file after a manual synchronization.
    /// </summary>
    public enum FinalState
    {
        Deleted,
        Updated,
        Created,
        Renamed,
        CreatedRenamed,
        Unchanged,
        Error,
        Conflict
    }
}
