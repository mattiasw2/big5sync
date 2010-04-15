/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
namespace Syncless.Notification
{
    /// <summary>
    /// The abstract class for Progress
    /// </summary>
    public abstract class Progress
    {
        /// <summary>
        /// Get and Set the State of the Progress
        /// </summary>
        public SyncState State
        {
            get;
            protected set;
        }
        /// <summary>
        /// Initialize the Progress
        /// </summary>
        /// <param name="tagName">Name of the tag</param>
        public Progress(string tagName)
        {
            State = SyncState.Started;
            TagName = tagName;
        }
        /// <summary>
        /// Get and Set the name of the tag
        /// </summary>
        public string TagName
        {
            get;
            set;
        }
        /// <summary>
        /// Get and set the message of the progress
        /// </summary>
        public string Message
        {
            get;
            set;
        }
        /// <summary>
        /// Method informing the Progress that a job complete
        /// </summary>
        public abstract void Complete();
        /// <summary>
        /// Method informing the Progress that a job fail
        /// </summary>
        public abstract void Fail();
        /// <summary>
        /// Inform the Progress that the Job is Cancel
        /// </summary>
        public abstract void Cancel();
        /// <summary>
        /// Inform the Observers that the progress have changed.
        /// </summary>
        public abstract void Update();
        /// <summary>
        /// Get the state if the Progress is cancelled.
        /// </summary>
        public bool IsCancel
        {
            get { return State == SyncState.Cancelled; }
        }

       
    }
}
