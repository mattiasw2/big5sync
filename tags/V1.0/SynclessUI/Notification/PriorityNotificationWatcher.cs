using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Syncless.Notification;
using Syncless.Core;

namespace SynclessUI.Notification
{
    internal class PriorityNotificationWatcher : IQueueObserver
    {
        private const int SLEEP_TIME = 10000;
        private Thread workerThread;
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);

        public PriorityNotificationWatcher()
        {
            ServiceLocator.UIPriorityQueue().AddObserver(this);

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

        }

    }
}
