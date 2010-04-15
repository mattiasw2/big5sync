/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using System;
using Syncless.Notification;
using System.Threading;
namespace Syncless.Core
{
    /// <summary>
    /// The Notification Queue Observer for the System Logic Layer.
    /// </summary>
    internal class LogicQueueObserver : IQueueObserver
    {
        /// <summary>
        /// Store the System Logic Layer.
        /// </summary>
        private readonly SystemLogicLayer _sll;
        /// <summary>
        /// Thread in charge of reading the notification queue.
        /// </summary>
        private readonly Thread _workerThread;
        /// <summary>
        /// Initialize and start the thread.
        /// </summary>
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);
        public LogicQueueObserver()
        {
            ServiceLocator.LogicLayerNotificationQueue().AddObserver(this);
            _sll = SystemLogicLayer.Instance;

            _workerThread = new Thread(Run);
        }
        /// <summary>
        /// Inform the Observer that something is added to the Queue
        /// </summary>
        public void Update()
        {
            _wh.Set();
        }
        public delegate void RunDel();
        /// <summary>
        /// Start the watcher
        /// </summary>
        public void Start()
        {
            _workerThread.Start();
        }
        /// <summary>
        /// Stop the Watcher.
        /// </summary>
        public void Stop()
        {
            _workerThread.Abort();
        }
        //Main Thread method.
        private void Run()
        {
            while (true)
            {
                //If the Queue does not have any notification, wait.
                if (!ServiceLocator.LogicLayerNotificationQueue().HasNotification())
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
                    try
                    {
                        AbstractNotification notification = ServiceLocator.LogicLayerNotificationQueue().Dequeue();
                        Handle(notification);
                    }
                    catch (Exception e)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.DEBUG_LOG).Write(e);
                    }
                }
            }

        }
        //Handle the different type of notification received.
        private void Handle(AbstractNotification notification)
        {
            switch (notification.NotificationCode)
            {
                case NotificationCode.MonitorPathNotification: //Inform the SLL that a new Path have been merged
                    {
                        MonitorPathNotification mpNotification = notification as MonitorPathNotification;
                        if (mpNotification == null) //Discard
                            return;
                        _sll.AddTagPath(mpNotification.TargetTag, mpNotification.TargetPath);
                    }
                    break;
                case NotificationCode.UnmonitorPathNotification: //Inform the SLL that a Path have been untag due to merging
                    {
                        UnMonitorPathNotification umpNotification = notification as UnMonitorPathNotification;
                        if (umpNotification == null)//Discard
                            return;
                        _sll.RemoveTagPath(umpNotification.TargetTag, umpNotification.TargetPath);
                    }
                    break;
                case NotificationCode.AddTagNotification: //Inform the SLL that a new Tag have been added due to merging
                    {
                        AddTagNotification atNotification = notification as AddTagNotification;
                        if (atNotification == null)//Discard
                            return;
                        _sll.AddTag(atNotification.Tag);
                    }
                    break;
                case NotificationCode.DeleteTagNotification: //Inform the SLL that a Tag have been deleted due to merging
                    {
                        RemoveTagNotification rmNotification = notification as RemoveTagNotification;
                        if (rmNotification == null)//Discard
                            return;

                        _sll.RemoveTag(rmNotification.Tag);
                    }
                    break;
                case NotificationCode.MonitorTagNotification: //Inform the SLL to start Monitoring a Tag(Called after a manual sync is completed)
                    {
                        MonitorTagNotification mtNotification = notification as MonitorTagNotification;
                        if (mtNotification == null)//Discard
                            return;
                        _sll.MonitorTag(mtNotification.Tagname);
                    }
                    break;
                case NotificationCode.TaggedPathDeletedNotification: //Inform the SLL that a tagged folder have been deleted. 
                    {
                        TaggedPathDeletedNotification tpdNotification = notification as TaggedPathDeletedNotification;
                        if (tpdNotification == null) // Discard
                            return;
                        _sll.Untag(tpdNotification.DeletedPaths);
                    }
                    break;
                case NotificationCode.SaveNotification: // Inform the SLL to save.
                    _sll.Save();
                    break;
            }
        }

    }
}
