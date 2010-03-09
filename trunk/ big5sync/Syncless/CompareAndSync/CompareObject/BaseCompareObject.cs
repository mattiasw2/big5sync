using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompareAndSync.CompareObject
{
    public abstract class BaseCompareObject
    {
        //Actual file
        private string _name;
        private long[] _creationTime;

       //Meta file
        private long[] _metaCreationTime;

        //All
        private bool[] _exists;
        private MetaChangeType?[] _changeType;
        private int[] _priority;

        protected BaseCompareObject(string name, int numOfPaths)
        {
            _name = name;
            _creationTime = new long[numOfPaths];
            _exists = new bool[numOfPaths];
            _changeType = new MetaChangeType?[numOfPaths];
            _priority = new int[numOfPaths];

            _metaCreationTime = new long[numOfPaths];
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

        public long[] MetaCreationTime
        {
            get { return _metaCreationTime; }
            set { _metaCreationTime = value; }
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
