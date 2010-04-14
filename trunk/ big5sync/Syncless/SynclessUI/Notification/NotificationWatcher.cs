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
    /// NotificationWatcher to dequeue any notification in the UINotificationQueue
    /// </summary>
    internal class NotificationWatcher : IQueueObserver
    {
        private MainWindow _main;

        private Thread workerThread;
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);

        /// <summary>
        /// Initializes the MainWindow 
        /// </summary>
        /// <param name="main">Reference to the MainWindow</param>
        public NotificationWatcher(MainWindow main)
        {
            _main = main;

            ServiceLocator.UINotificationQueue().AddObserver(this);

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
        /// Run method of the workerThread, constantly gets notifications from the UINotificationQueue if there is,
        /// if there isnt, wait until wait handle has a signal.
        /// </summary>
        private void Run()
        {
            while (true)
            {
                // if no notification found, put into wait state
                if (!ServiceLocator.UINotificationQueue().HasNotification())
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
                        AbstractNotification notification = ServiceLocator.UINotificationQueue().Dequeue();
                        Handle(notification);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        //Handle Exception by printing the debug log.
                    }
                }
            }

        }

        /// <summary>
        /// Handles any dequeued notification from the UINotificationQueue by notifying the UI
        /// </summary>
        /// <param name="notification">Notification to handle</param>
        private void Handle(AbstractNotification notification)
        {
            // Handles Sync Start Notification
            if (notification.NotificationCode.Equals(NotificationCode.SyncStartNotification))
            {
                SyncStartNotification ssNotification = notification as SyncStartNotification;
                if (ssNotification != null)
                {
                    _main.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        _main.CurrentProgress = ssNotification.Progress;
                        new SyncProgressWatcher(_main, ssNotification.TagName, ssNotification.Progress);
                    }));
                }
            }
            // Handles Sync Complete Notification
            else if (notification.NotificationCode.Equals(NotificationCode.SyncCompleteNotification))
            {
                SyncCompleteNotification scNotification = notification as SyncCompleteNotification;
                if (scNotification != null)
                {
                    _main.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (Action)(() =>
                        {
                            _main.TagChanged(scNotification.TagName);
                        }));
                }
            }
            // Handles Nothing to Sync Notification
            else if (notification.NotificationCode.Equals(NotificationCode.NothingToSyncNotification))
            {
                NothingToSyncNotification ntsNotification = notification as NothingToSyncNotification;
                if (ntsNotification != null)
                {
                    _main.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (Action)(() =>
                        {

                            _main.NotifyNothingToSync(ntsNotification.TagName);
                        }));
                }
            }
            // Handles Auto Sync Complete Notification
            else if (notification.NotificationCode.Equals(NotificationCode.AutoSyncCompleteNotification))
            {
                AutoSyncCompleteNotification ascNotification = notification as AutoSyncCompleteNotification;
                if (ascNotification != null)
                {
                    _main.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (Action)(() =>
                        {
                            _main.NotifyAutoSyncComplete(ascNotification.Path);
                        }));
                }
            }
        }
    }
}
