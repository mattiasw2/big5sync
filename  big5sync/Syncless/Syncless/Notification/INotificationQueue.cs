namespace Syncless.Notification
{
    /// <summary>
    /// INotificationQueue is an interface for implementing a notification queue. Classes which implement the
    /// interface must implement Enqueue, Dequeue, AddObserver and HasNotification methods.
    /// </summary>
    public interface INotificationQueue 
    {
        /// <summary>
        /// Enqueues a <see cref="AbstractNotification">AbstractNotification</see> object
        /// </summary>
        /// <param name="notification">The <see cref="AbstractNotification">AbstractNotification</see> object 
        /// that represents the notification to be enqueued</param>
        /// <returns>true if the notification is enqueued; otherwise, false</returns>
        bool Enqueue(AbstractNotification notification);

        /// <summary>
        /// Dequeues a <see cref="AbstractNotification">AbstractNotification</see> object
        /// </summary>
        /// <returns>the first notification in the queue</returns>
        AbstractNotification Dequeue();

        /// <summary>
        /// Adds an <see cref="IQueueObserver">IQueueObserver</see> object to the list of observer objects
        /// </summary>
        /// <param name="obs"></param>
        void AddObserver(IQueueObserver obs);

        /// <summary>
        /// Checks if there is still notification objects in the queue
        /// </summary>
        /// <returns></returns>
        bool HasNotification();
    }
}
