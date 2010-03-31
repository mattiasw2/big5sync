namespace Syncless.CompareAndSync.Request
{
    public class CancelSyncRequest
    {
        private readonly string _tagName;

        public CancelSyncRequest(string tagName)
        {
            _tagName = tagName;
        }

        public string TagName
        {
            get { return _tagName; }
        }
    }
}
