using Syncless.Tagging;

namespace Syncless.Notification
{
    /// <summary>
    /// The object for Preview Progress
    /// </summary>
    public class PreviewProgress : Progress
    {
        /// <summary>
        /// Initialize the Preview Progress Object
        /// </summary>
        /// <param name="tagName">name of the <see cref="Tag"/></param>
        public PreviewProgress(string tagName):base(tagName)
        {


        }
        /// <summary>
        /// Override the parent.
        /// </summary>
        public override void Complete()
        {
            //Do nothing
        }
        /// <summary>
        /// Override the fail.
        /// </summary>
        public override void Fail()
        {
            //Do nothing
        }
        /// <summary>
        /// Set the State to cancel
        /// </summary>
        public override void Cancel()
        {
            State = SyncState.Cancelled;
        }
        /// <summary>
        /// Override the cancel
        /// </summary>
        public override void Update()
        {
            //DO nothing
        }
    }
}
