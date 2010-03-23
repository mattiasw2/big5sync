using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Notification;
using System.Threading;

namespace Syncless.Core
{
    internal class NotificationWatcher : IQueueObserver
    {
        private const int SLEEP_TIME = 10000;
        private Thread workerThread;
        public NotificationWatcher()
        {
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
                        //Handle Exception by printing the debug log.
                    }
                }
            }

        }

        private void Handle(AbstractNotification notification)
        {   

        }

    }
}
