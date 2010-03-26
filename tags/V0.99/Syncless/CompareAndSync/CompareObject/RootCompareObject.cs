namespace Syncless.CompareAndSync.CompareObject
{
    public class RootCompareObject : FolderCompareObject
    {
        private readonly string[] _paths;

        public RootCompareObject(string[] paths)
            : base(null, paths.Length, null)
        {
            _paths = paths;
        }

        public string[] Paths
        {
            get { return _paths; }
        }
    }
}
