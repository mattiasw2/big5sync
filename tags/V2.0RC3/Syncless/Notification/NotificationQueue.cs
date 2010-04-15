/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace Syncless.Notification
{
    /// <summary>
    /// The Notification Queue that provide implementation for Enqueuing and DeQueuing of Notification
    /// </summary>
    public class NotificationQueue : INotificationQueue
    {
        /// <summary>
        /// Notification Queue object
        /// </summary>
        private Queue<AbstractNotification> _notificationQueue;
        /// <summary>
        /// Observer List 
        /// </summary>
        private List<IQueueObserver> _observerList;
        /// <summary>
        /// Initialize the Notification QUeue
        /// </summary>
        public NotificationQueue()
        {
            _notificationQueue = new Queue<AbstractNotification>();
            _observerList = new List<IQueueObserver>();
        }
        /// <summary>
        /// Enqueue a Notification to the Queue and inform all the observer that a <see cref="AbstractNotification"/> is added to the queue
        /// </summary>
        /// <param name="notification">The notification to enqueued</param>
        /// <returns>true if the Notification is enqueue. false if the Queue already contain the Notification object.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Enqueue(AbstractNotification notification)
        {
            if (_notificationQueue.Contains(notification))
            {   
                return false;
            }
            _notificationQueue.Enqueue(notification);
            NotifyAll();
            return true;
        }
        /// <summary>
        /// Dequeue the first Notification from the Queue and inform all observer that a <see cref="AbstractNotification"/> is removed from the queue
        /// </summary>
        /// <returns>The <see cref="AbstractNotification"/> that is dequeued</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public AbstractNotification Dequeue()
        {
            if (_notificationQueue.Count == 0)
            {
                NotifyAll();
                return null;
            }
            NotifyAll();
            return _notificationQueue.Dequeue();
        }
        /// <summary>
        /// Register an observer to the queue
        /// </summary>
        /// <param name="obs">The observer object</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddObserver(IQueueObserver obs)
        {
            _observerList.Add(obs);
        }

        /// <summary>
        /// Notify all observer that the queue have changed.
        /// </summary>
        private void NotifyAll()
        {
            foreach (IQueueObserver obs in _observerList)
            {
                UpdateDelegate upDelegate = new UpdateDelegate(obs.Update);
                upDelegate.BeginInvoke(null, null);
            }
        }
        private delegate void UpdateDelegate();

        #region INotificationQueue Members
        /// <summary>
        /// Provide the method to check if the queue have a notification
        /// </summary>
        /// <returns></returns>
        public bool HasNotification()
        {
            return _notificationQueue.Count != 0;
        }
        /// <summary>
        /// Check if the queue contain the notification
        /// </summary>
        /// <param name="notification">the notification to check</param>
        /// <returns>true if the queue, otherwise false</returns>
        public bool Contains(AbstractNotification notification)
        {
            return _notificationQueue.Contains(notification);
        }

        #endregion
    }
}
