using System;
using System.Threading;
using System.Windows.Threading;
using Syncless.Core;
using Syncless.Notification;
using Syncless.Notification.UINotification;

namespace SynclessUI.Notification
{
    internal class NotificationWatcher : IQueueObserver
    {
        private MainWindow _main;

        private const int SLEEP_TIME = 10000;
        private Thread workerThread;
        
        public NotificationWatcher(MainWindow main)
        {
            _main = main;

            ServiceLocator.UINotificationQueue().AddObserver(this);

            workerThread = new Thread(Run);
        }
        public void Update()
        {
            workerThread.Interrupt();
        }
        public delegate void RunDel();

        public void Start()
        {
            workerThread.Start();
        }
        public void Stop()
        {
            workerThread.Abort();
        }
        private void Run()
        {
            while (true)
            {
                if (!ServiceLocator.UINotificationQueue().HasNotification())
                {
                    try
                    {
                        Thread.Sleep(SLEEP_TIME);
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

        private void Handle(AbstractNotification notification)
        {
            if (notification.NotificationCode.Equals(NotificationCode.SYNC_START_NOTIFICATION))
            {
                SyncStartNotification ssNotification = notification as SyncStartNotification;
                if (ssNotification != null)
                {
                    SyncProgressWatcher watcher = new SyncProgressWatcher(_main, ssNotification.TagName,
                                                                          ssNotification.Progress);
                    
                }
            } else if(notification.NotificationCode.Equals(NotificationCode.SYNC_COMPLETE_NOTIFICATION))
            {
                SyncCompleteNotification scNotification = notification as SyncCompleteNotification;
                if(scNotification != null)
                {
                    
                    
                    _main.LblStatusText.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (Action)(() =>
                        {
                            _main.NotifySyncCompletion(scNotification.TagName);
                            _main.TagChanged();
                            Console.WriteLine("Sync Complete: Calling Tag Changed");
                        }));
                }
            }
            else if (notification.NotificationCode.Equals(NotificationCode.NOTHING_TO_SYNC_NOTIFICATION))
            {
                NothingToSyncNotification ntsNotification = notification as NothingToSyncNotification;
                if (ntsNotification != null)
                {
                    _main.LblStatusText.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (Action)(() =>
                        {
                            _main.NotifyNothingToSync(ntsNotification.TagName);
                        }));
                }
            }
        }
    }
}
