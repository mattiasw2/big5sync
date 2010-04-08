using System.Collections.Generic;
using Syncless.CompareAndSync.Manual;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.CompareAndSync.Manual.Visitor;
using Syncless.CompareAndSync.Request;
using Syncless.CompareAndSync.Seamless;
using Syncless.Core;
using Syncless.Notification;
using Syncless.Notification.UINotification;

namespace Syncless.CompareAndSync
{
    public class CompareAndSyncController
    {
        private static CompareAndSyncController _instance;
        private PreviewProgress _currPreviewProgress;

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

        public bool IsQueued(string tagName)
        {
            return ManualQueueControl.Instance.IsQueued(tagName);
        }

        public bool IsSyncing(string tagName)
        {
            return ManualQueueControl.Instance.IsSyncing(tagName);
        }

        public bool IsQueuedOrSyncing(string tagName)
        {
            return ManualQueueControl.Instance.IsQueuedOrSyncing(tagName);
        }

        #endregion

        #region Manual Preview

        public RootCompareObject Preview(ManualCompareRequest request)
        {
            _currPreviewProgress = new PreviewProgress(request.TagName);
            RootCompareObject rco = ManualSyncer.Compare(request, _currPreviewProgress);
            _currPreviewProgress = null;
            return rco;
        }

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

        public void Sync(AutoSyncRequest request)
        {
            SeamlessQueueControl.Instance.AddSyncJob(request);
        }

        #endregion

        #region Termination

        public bool PrepareForTermination()
        {
            return (ManualQueueControl.Instance.PrepareForTermination() &&
                    SeamlessQueueControl.Instance.PrepareForTermination());
        }

        public void Terminate()
        {
            ManualQueueControl.Instance.Terminate();
            SeamlessQueueControl.Instance.Terminate();
        }

        #endregion

    }
}
