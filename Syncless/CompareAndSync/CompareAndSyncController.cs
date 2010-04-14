/*
 * 
 * Author: Soh Yuan Chin
 * 
 */

using Syncless.CompareAndSync.Manual;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.CompareAndSync.Request;
using Syncless.CompareAndSync.Seamless;
using Syncless.Notification;

namespace Syncless.CompareAndSync
{
    /// <summary>
    /// Facade for other classes to communicate with the CompareAndSync component.
    /// </summary>
    public class CompareAndSyncController
    {
        private static CompareAndSyncController _instance;
        private PreviewProgress _currPreviewProgress;

        /// <summary>
        /// Singleton pattern. Returns the instance of CompareAndSyncController if it exists. Otherwise, it creates a new instance.
        /// </summary>
        public static CompareAndSyncController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CompareAndSyncController();
                }
                return _instance;
            }
        }

        private CompareAndSyncController()
        {
        }

        #region Manual Synchronization

        /// <summary>
        /// Adds a ManualSyncRequest job to the single-instance ManualQueueControl.
        /// </summary>
        /// <param name="request">ManualSyncRequest request to add to the queue.</param>
        public void Sync(ManualSyncRequest request)
        {
            ManualQueueControl.Instance.AddSyncJob(request);
        }

        /// <summary>
        /// Returns a boolean indicating if the given request has been cancelled.
        /// </summary>
        /// <param name="request">The CancelSyncRequest containing information on which job to cancel.</param>
        /// <returns>A boolean indicating if the given request has been cancelled.</returns>
        public bool Cancel(CancelSyncRequest request)
        {
            return ManualQueueControl.Instance.CancelSyncJob(request);
        }

        /// <summary>
        /// Check is a tag is currently in the manual queue.
        /// </summary>
        /// <param name="tagName">The tag name to check for.</param>
        /// <returns>True if the tag name is currently in queued, else false.</returns>
        public bool IsQueued(string tagName)
        {
            return ManualQueueControl.Instance.IsQueued(tagName);
        }

        /// <summary>
        /// Check if a tag is currently being synchronized.
        /// </summary>
        /// <param name="tagName">The tag name to check for.</param>
        /// <returns>True if the tag name is currently being synchronized.</returns>
        public bool IsSyncing(string tagName)
        {
            return ManualQueueControl.Instance.IsSyncing(tagName);
        }

        /// <summary>
        /// Check if a tag is currently being queued or synced.
        /// </summary>
        /// <param name="tagName">The tag name to check for.</param>
        /// <returns>True if the tag name is being queued or being synchronized.</returns>
        public bool IsQueuedOrSyncing(string tagName)
        {
            return ManualQueueControl.Instance.IsQueuedOrSyncing(tagName);
        }

        #endregion

        #region Manual Preview

        /// <summary>
        /// The method is called when a preview is request.
        /// </summary>
        /// <param name="request">The <see cref="ManualCompareRequest"/> request to compare.</param>
        /// <returns>The <see cref="RootCompareObject"/> containing information of the comparison.</returns>
        public RootCompareObject Preview(ManualCompareRequest request)
        {
            _currPreviewProgress = new PreviewProgress(request.TagName);
            RootCompareObject rco = ManualSyncer.Compare(request, _currPreviewProgress);
            _currPreviewProgress = null;
            return rco;
        }

        /// <summary>
        /// Cancels the currently previewing job.
        /// </summary>
        /// <param name="tagName">The tag name to cancel.</param>
        public void CancelPreview(string tagName)
        {
            if (_currPreviewProgress != null && _currPreviewProgress.TagName == tagName)
            {
                _currPreviewProgress.Cancel();
                _currPreviewProgress = null;
            }
        }

        #endregion

        #region Seamless Synchronization

        /// <summary>
        /// The method is called when there is an auto/seamless request.
        /// </summary>
        /// <param name="request">The <see cref="AutoSyncRequest"/> to synchronize.</param>
        public void Sync(AutoSyncRequest request)
        {
            SeamlessQueueControl.Instance.AddSyncJob(request);
        }

        #endregion

        #region Termination

        /// <summary>
        /// The method is called when preparing for termination.
        /// </summary>
        /// <returns>Returns true if termination is possible, and false if it is not.</returns>
        public bool PrepareForTermination()
        {
            return (ManualQueueControl.Instance.PrepareForTermination() &&
                    SeamlessQueueControl.Instance.PrepareForTermination());
        }

        /// <summary>
        /// Terminates both Manual and Seamless queue controllers.
        /// </summary>
        public void Terminate()
        {
            ManualQueueControl.Instance.Terminate();
            SeamlessQueueControl.Instance.Terminate();
        }

        #endregion

    }
}
