using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Notification;
using System.Threading;
using Syncless.Notification.SLLNotification;
namespace Syncless.Core
{
    internal class LogicQueueObserver : IQueueObserver
    {
        private SystemLogicLayer _sll;
        private Thread workerThread;
        public LogicQueueObserver()
        {
            ServiceLocator.LogicLayerNotificationQueue().AddObserver(this);
            _sll = SystemLogicLayer.Instance;

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
                if (!ServiceLocator.LogicLayerNotificationQueue().HasNotification())
                {
                    try
                    {
                        Thread.Sleep(10000);
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
                        AbstractNotification notification = ServiceLocator.LogicLayerNotificationQueue().Dequeue();
                        Handle(notification);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Handle(e);
                    }
                }
            }

        }

        private void Handle(AbstractNotification notification)
        {
            if (notification.NotificationCode.Equals(NotificationCode.MONITOR_PATH_NOTIFICATION))
            {
                MonitorPathNotification mpNotification = notification as MonitorPathNotification;
                if (mpNotification == null)
                {
                    //Discard , The notificaiton is wrong
                    return;
                }
                _sll.AddTagPath(mpNotification.TargetTag, mpNotification.TargetPath);
            }
            else if (notification.NotificationCode.Equals(NotificationCode.UNMONITOR_PATH_NOTIFICATION))
            {
                UnMonitorPathNotification umpNotification = notification as UnMonitorPathNotification;
                if (umpNotification == null)
                {
                    //Discard
                    return;
                }
                _sll.RemoveTagPath(umpNotification.TargetTag, umpNotification.TargetPath);
            }
            else if (notification.NotificationCode.Equals(NotificationCode.ADD_TAG_NOTIFICATION))
            {
                AddTagNotification atNotification = notification as AddTagNotification;
                if (atNotification == null)
                {
                    //Discard
                    return;
                }
                _sll.AddTag(atNotification.Tag);
            }
            else if (notification.NotificationCode.Equals(NotificationCode.DEL_TAG_NOTIFICATION))
            {
                RemoveTagNotification rmNotification = notification as RemoveTagNotification;
                if (rmNotification == null)
                {
                    //Discard
                    return;
                }
                _sll.RemoveTag(rmNotification.Tag);
            }

            
        }
        
    }
}
