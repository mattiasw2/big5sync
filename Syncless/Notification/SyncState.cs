namespace Syncless.Notification
{
    public enum SyncState
    {
        NotSyncing, //Not Syncing
        Queued, //Seldom use
        Started, //Sync Started 
        Analyzing, //Building Tree + Comparing
        Synchronizing, //Copying files
        Finalizing, //Writing XML
        Finished, //Finished
        Cancelled , //Cancelled
        Unknown
    }

}
