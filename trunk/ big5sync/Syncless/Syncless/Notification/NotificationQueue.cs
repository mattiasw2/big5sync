using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
namespace Syncless.Notification
{
    public class NotificationQueue : INotificationQueue
    {
        private Queue<AbstractNotification> _notificationQueue;
        private List<QueueObserver> _observerList;
        public NotificationQueue()
        {
            _notificationQueue = new Queue<AbstractNotification>();
            _observerList = new List<QueueObserver>();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Enqueue(AbstractNotification notification)
        {
            if (_notificationQueue.Contains(notification))
            {   
                return false;
            }
            else
            {
                _notificationQueue.Enqueue(notification);
                NotifyAll();
                return true;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public AbstractNotification Dequeue(AbstractNotification notification)
        {
            if (_notificationQueue.Count == 0)
            {
                NotifyAll();
                return null;
            }
            NotifyAll();
            return _notificationQueue.Dequeue();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddObserver(QueueObserver obs)
        {
            _observerList.Add(obs);
        }

        private void NotifyAll()
        {
            foreach (QueueObserver obs in _observerList)
            {
                obs.Update();
            }
        }
    }
}
