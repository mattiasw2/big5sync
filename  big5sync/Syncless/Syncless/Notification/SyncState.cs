namespace Syncless.Notification
{
    /// <summary>
    /// SyncState enum provides the enumerations of sync state.
    /// </summary>
    public enum SyncState
    {
        NotSyncing, //Not Syncing
        Queued, //Seldom use
        Started, //Sync Started 
        Analyzing, //Building Tree + Comparing
        Synchronizing, //Copying files
        Finalizing, //Writing XML
        Finished, //Finished
        Cancelled, //Cancelled
        Unknown
    }

}
