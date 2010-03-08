using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestNewComparer.CompareAndSync
{
    public class FolderCompareObject : CompareObject
    {
        private List<CompareObject> _objectList;

        public List<CompareObject> ObjectList
        {
            get { return _objectList; }
            set { _objectList = value; }
        }

        public FolderCompareObject(string name , int count)
            : base(name , count)
        {
            _objectList = new List<CompareObject>();
        }

        public bool Contains(string name)
        {
            return GetCompareObject(name) != null;
        }
        public CompareObject GetCompareObject(string name)
        {
            foreach (CompareObject o in _objectList)
            {
                if (o.Name.Equals(name))
                {
                    return o;
                }
            }
            return null;
        }
        public void AddChild(CompareObject o)
        {
            _objectList.Add(o);
            
        }


    }
}
