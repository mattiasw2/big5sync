namespace Syncless.Notification
{
    /// <summary>
    /// The interface to Observe a Progress
    /// </summary>
    public interface ISyncProgressObserver
    {
        /// <summary>
        /// Provided for Progress to inform Progress State changed
        /// </summary>
        void StateChanged();
        /// <summary>
        /// Provided for Progress to inform Progress Changed
        /// </summary>
        void ProgressChanged();
        /// <summary>
        /// Provided for Progress to inform of Sync Complete
        /// </summary>
        void SyncComplete();
    }
}
