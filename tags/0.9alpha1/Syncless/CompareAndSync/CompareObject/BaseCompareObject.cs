using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.CompareObject
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
        //private List<string> _newNames;
        private string _newName;
        private FolderCompareObject _parent;
        private ToDo? _todo;
        private bool _invalid;

        //private bool? _ancestorRenamed;

        protected BaseCompareObject(string name, int numOfPaths, FolderCompareObject parent)
        {
            _name = name;
            _creationTime = new long[numOfPaths];
            _exists = new bool[numOfPaths];
            _finalState = new FinalState?[numOfPaths];
            _metaCreationTime = new long[numOfPaths];
            _metaExists = new bool[numOfPaths];
            _changeType = new MetaChangeType?[numOfPaths];
            _priority = new int[numOfPaths];
            //_newNames = new List<string>();
            _parent = parent;
            _invalid = false;
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

        /*
        public List<string> NewNames
        {
            get { return _newNames; }
            set { _newNames = value; }
        }*/

        public string NewName
        {
            get { return _newName; }
            set { _newName = value; }
        }

        public FolderCompareObject Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public ToDo? ToDoAction
        {
            get { return _todo; }
            set { _todo = value; }
        }

        public bool Invalid
        {
            get { return _invalid; }
            set { _invalid = value; }
        }

        /*
        public bool? AncestorOrItselfRenamed
        {
            get
            {
                if (_ancestorRenamed.HasValue)
                    return _ancestorRenamed;
                else
                    if (this is RootCompareObject)
                        return false;
                    else
                        return _parent.AncestorOrItselfRenamed;
            }
            set { _ancestorRenamed = value; }
        }*/

        public abstract string GetFullParentPath(int index);

    }
}
