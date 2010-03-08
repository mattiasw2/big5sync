using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompareAndSync.CompareObject
{
    public class FolderCompareObject : BaseCompareObject
    {
        private List<BaseCompareObject> _contents;

        public FolderCompareObject(string name, int numOfPaths)
            : base(name, numOfPaths)
        {
            _contents = new List<BaseCompareObject>();
        }

        public void AddChild(BaseCompareObject child)
        {
            _contents.Add(child);
        }

        public BaseCompareObject GetChild(string name)
        {
            //_contents.BinarySearch(IComparer); Dictionary?
            return null;
        }

        public bool ContainsChild(string name)
        {
            return GetChild(name) != null;
        }

        public List<BaseCompareObject> Contents
        {
            get { return _contents; }
            set { _contents = value; }
        }
    }
}
