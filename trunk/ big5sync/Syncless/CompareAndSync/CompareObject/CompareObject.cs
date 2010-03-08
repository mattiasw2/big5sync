using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompareAndSync.CompareObject
{
    public abstract class BaseCompareObject
    {
        protected string _name;
        protected long[] _creationTime;
        protected bool[] _exists;
        protected MetaChangeType?[] _changeType;
        protected int[] _priority;

        protected BaseCompareObject(string name, int numOfPaths)
        {
            _name = name;
            _creationTime = new long[numOfPaths];
            _exists = new bool[numOfPaths];
            _changeType = new MetaChangeType?[numOfPaths];
            _priority = new int[numOfPaths];
        }

        public string Name
        {
            get { return _name; }
        }

        public long[] CreationTime
        {
            get { return _creationTime; }
            set { _creationTime = value; }
        }

        public bool[] ExistsArray
        {
            get { return _exists; }
            set { _exists = value; }
        }

        public MetaChangeType?[] ChangeTypeArray
        {
            get { return _changeType; }
            set { _changeType = value; }
        }

        public int[] Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

    }
}
