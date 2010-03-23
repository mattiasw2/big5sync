using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Enum;
using System.IO;
using System.Diagnostics;

namespace Syncless.CompareAndSync.CompareObject
{
    public abstract class BaseCompareObject
    {
        //Actual
        private string _name;
        private long[] _creationTime;
        private bool[] _exists;

        //Meta
        private long[] _metaCreationTime;
        private bool[] _metaExists;

        //All       
        private MetaChangeType?[] _changeType;
        private FinalState?[] _finalState;
        private int[] _priority;
        private FolderCompareObject _parent;
        private bool _invalid;
        private string _newName;

        //Todo
        private ToDo?[] _toDoAction;

        public ToDo?[] ToDoAction
        {
            get { return _toDoAction; }
            set { _toDoAction = value; }
        }
        private long[] _toDoTimestamp;

        public long[] ToDoTimestamp
        {
            get { return _toDoTimestamp; }
            set { _toDoTimestamp = value; }
        }

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
            _parent = parent;
            _invalid = false;

            _toDoAction = new ToDo?[numOfPaths];
            _toDoTimestamp = new long[numOfPaths];
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

        public FolderCompareObject Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public bool Invalid
        {
            get { return _invalid; }
            set { _invalid = value; }
        }

        public string NewName
        {
            get { return _newName; }
            set { _newName = value; }
        }

        public string GetSmartParentPath(int index)
        {
            if (Parent == null)
                return "ROOT"; //Will throw exception in future

            RootCompareObject rco = null;
            if ((rco = Parent as RootCompareObject) != null)
                return rco.Paths[index];
            else
            {
                if (Parent.ChangeType[index] == MetaChangeType.Rename)
                    return Path.Combine(Parent.GetSmartParentPath(index), Parent.NewName);
                else
                    return Path.Combine(Parent.GetSmartParentPath(index), Parent.FinalState[index] == Syncless.CompareAndSync.Enum.FinalState.Renamed ? Parent.NewName : Parent.Name);
            }
        }

        //public abstract string GetSmartParentPath(int index);
        //public abstract string GetFullParentPath(int index);
    }
}
