namespace Syncless.CompareAndSync.Enum
{
    public enum FinalState
    {
        Deleted,
        Updated,
        Created,
        Renamed,
        CreatedRenamed,
        /*Propagated,*/
        Unchanged,
        Error,
        Conflict
    }
}
