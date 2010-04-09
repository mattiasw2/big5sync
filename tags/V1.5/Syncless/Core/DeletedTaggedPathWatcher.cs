using System.Collections.Generic;
using System.Threading;
using Syncless.Notification;

namespace Syncless.Core
{
    /// <summary>
    /// DeletedTaggedPathWatcher class detects the list of deleted tagged paths and sends notification to
    /// <see cref="SystemLogicLayer">SystemLogicLayer</see>.
    /// </summary>
    public class DeletedTaggedPathWatcher
    {
        private Thread _watcher;

        /// <summary>
        /// Starts the watcher thread
        /// </summary>
        public void Start()
        {
            _watcher = new Thread(Run);
            _watcher.Start();
        }

        /// <summary>
        /// Stops the watcher thread
        /// </summary>
        public void Stop()
        {
            _watcher.Abort();
        }

        /// <summary>
        /// Runs the watcher thread
        /// </summary>
        private void Run()
        {
            while (true)
            {
                try
                {
                    List<string> deletedPaths = SystemLogicLayer.Instance.FindAllDeletedPaths();
                    if (deletedPaths.Count > 0)
                    {
                        ServiceLocator.LogicLayerNotificationQueue().Enqueue(new TaggedPathDeletedNotification(deletedPaths));
                    }
                    Thread.Sleep(30000);
                }
                catch (ThreadInterruptedException)
                {

                }
                catch (ThreadAbortException)
                {
                    return;
                }
            }
        }
    }
}
