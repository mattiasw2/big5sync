using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompareAndSync.CompareObject
{
    public abstract class BaseCompareObject
    {
        protected string _name;
        protected long _creationTime;
        protected bool[] _exists;
        protected MetaChangeType?[] _changeType;
        protected int[] _priority;

        protected BaseCompareObject(string name, long creationTime, int numOfPaths)
        {
            _name = name;
            _creationTime = creationTime;
            _exists = new bool[numOfPaths];
            _changeType = new MetaChangeType?[numOfPaths];
            _priority = new int[numOfPaths];
        }

        public string Name
        {
            get { return _name; }
        }

        public long CreationTime
        {
            get { return _creationTime; }
        }

        public bool[] ExistsArray
        {
            get { return _exists; }
        }

        public MetaChangeType?[] ChangeTypeArray
        {
            get { return _changeType; }
        }

        public bool Exists(int fileIndex)
        {
            return _exists[fileIndex];
        }

        public MetaChangeType? GetChangeType(int fileIndex)
        {
            return _changeType[fileIndex];
        }

    }
}
