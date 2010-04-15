/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using System;
using System.Threading;
using System.Windows.Threading;
using Syncless.Core;
using Syncless.Notification;

namespace SynclessUI.Notification
{
    /// <summary>
    /// PriorityNotificationWatcher to dequeue any notification in the UIPriorityQueue
    /// </summary>
    internal class PriorityNotificationWatcher : IQueueObserver
    {
        private Thread workerThread;
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);
        private readonly MainWindow _main;
        
        /// <summary>
        /// Initializes the MainWindow 
        /// </summary>
        /// <param name="main">Reference to the MainWindow</param>
        public PriorityNotificationWatcher(MainWindow main)
        {
            ServiceLocator.UIPriorityQueue().AddObserver(this);
            _main = main;
            workerThread = new Thread(Run);
        }

        /// <summary>
        /// Update the wait handle to signalled.
        /// </summary>
        public void Update()
        {
            _wh.Set(); 
        }

        /// <summary>
        /// Starts the workerThread
        /// </summary>
        public void Start()
        {
            workerThread.Start();
        }

        /// <summary>
        /// Stop/Aborts the workerThread
        /// </summary>
        public void Stop()
        {
            workerThread.Abort();
        }

        /// <summary>
        /// Run method of the workerThread, constantly gets notifications from the UIPriorityQueue if there is,
        /// if there isnt, wait until wait handle has a signal.
        /// </summary>
        private void Run()
        {
            while (true)
            {
                // if no notification found, put into wait state
                if (!ServiceLocator.UIPriorityQueue().HasNotification())
                {
                    try
                    {
                        _wh.WaitOne();
                        continue;
                    }
                    catch (ThreadInterruptedException)
                    {

                    }
                    catch (ThreadAbortException)
                    {
                        break;
                    }
                }
                else
                {
                    // retrieve the next notification and handle it
                    try
                    {
                        AbstractNotification notification = ServiceLocator.UIPriorityQueue().Dequeue();
                        Handle(notification);
                    }
                    catch (Exception) {}
                }
            }
        }

        /// <summary>
        /// Handles any dequeued notification from the UIPriorityQueue by notifying the UI
        /// </summary>
        /// <param name="notification"></param>
        private void Handle(AbstractNotification notification)
        {
            // Handles Cancel Sync Notification
            if (notification.NotificationCode == NotificationCode.CancelSyncNotification)
            {
                
                CancelSyncNotification csNotification = notification as CancelSyncNotification;
                if (csNotification != null)
                {
                    if (csNotification.IsCancel)
                        _main.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                        {
                            _main.NotifyCancelComplete(csNotification.TagName);
                        }));
                }
            }
        }

    }
}
