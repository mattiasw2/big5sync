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
    internal class PriorityNotificationWatcher : IQueueObserver
    {
        private Thread workerThread;
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);
        private readonly MainWindow _main;
        public PriorityNotificationWatcher(MainWindow main)
        {
            ServiceLocator.UIPriorityQueue().AddObserver(this);
            _main = main;
            workerThread = new Thread(Run);
        }
        public void Update()
        {
            _wh.Set();
            //workerThread.Interrupt();
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
                if (!ServiceLocator.UIPriorityQueue().HasNotification())
                {
                    try
                    {
                        _wh.WaitOne();
                        //Thread.Sleep(SLEEP_TIME);
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
                        AbstractNotification notification = ServiceLocator.UIPriorityQueue().Dequeue();
                        Handle(notification);
                    }
                    catch (Exception)
                    {

                    }
                }
            }

        }

        private void Handle(AbstractNotification notification)
        {
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
