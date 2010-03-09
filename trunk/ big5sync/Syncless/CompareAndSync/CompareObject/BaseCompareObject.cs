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
        private bool[] _exists;

       //Meta file
        private long[] _metaCreationTime;
        private bool[] _metaExists;

        //All       
        private MetaChangeType?[] _changeType;
        private FinalState?[] _finalState;
        private int[] _priority;
        private List<string> _newNames;

        protected BaseCompareObject(string name, int numOfPaths)
        {
            _name = name;
            _creationTime = new long[numOfPaths];
            _exists = new bool[numOfPaths];
            _metaCreationTime = new long[numOfPaths];
            _metaExists = new bool[numOfPaths];
            _changeType = new MetaChangeType?[numOfPaths];
            _priority = new int[numOfPaths];
            _newNames = new List<string>();            
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

        public bool[] Exists
        {
            get { return _exists; }
            set { _exists = value; }
        }

        public bool[] MetaExists
        {
            get { return _metaExists; }
            set { _metaExists = value; }
        }

        public MetaChangeType?[] ChangeType
        {
            get { return _changeType; }
            set { _changeType = value; }
        }

        public FinalState?[] FinalState
        {
            get { return _finalState; }
            set { _finalState = value; }
        }

        public int[] Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        public List<string> NewNames
        {
            get { return _newNames; }
            set { _newNames = value; }
        }

    }
}
