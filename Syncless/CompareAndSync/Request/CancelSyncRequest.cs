/*
 * 
 * Author: Soh Yuan Chin
 * 
 */

namespace Syncless.CompareAndSync.Request
{
    /// <summary>
    /// <c>CancelSyncRequest</c> is used for cancelling sync requests.
    /// </summary>
    public class CancelSyncRequest : Request
    {
        private readonly string _tagName;

        /// <summary>
        /// Instantiates an instance of <c>CancelSyncRequest</c>.
        /// </summary>
        /// <param name="tagName">The tag name to cancel.</param>
        public CancelSyncRequest(string tagName)
        {
            _tagName = tagName;
        }

        /// <summary>
        /// Gets the tag name.
        /// </summary>
        public string TagName
        {
            get { return _tagName; }
        }
    }
}
