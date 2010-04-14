/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
namespace Syncless.Notification
{
    /// <summary>
    /// SyncState enum provides the enumerations of sync state.
    /// </summary>
    public enum SyncState
    {
        /// <summary>
        /// Not Synchronizing
        /// </summary>
        NotSyncing, 
        /// <summary>
        /// Queued
        /// </summary>
        Queued,
        /// <summary>
        /// Sync started
        /// </summary>
        Started, 
        /// <summary>
        /// Analyzing (Building tree , reading Metadata)
        /// </summary>
        Analyzing,
        /// <summary>
        /// Synchronizing (Copying the file)
        /// </summary>
        Synchronizing,
        /// <summary>
        /// Writing Meta data
        /// </summary>
        Finalizing,
        /// <summary>
        /// The Sync is complete
        /// </summary>
        Finished, 
        /// <summary>
        /// The Sync is cancelled
        /// </summary>
        Cancelled,
        /// <summary>
        /// Unknown State
        /// </summary>
        Unknown
    }

}
