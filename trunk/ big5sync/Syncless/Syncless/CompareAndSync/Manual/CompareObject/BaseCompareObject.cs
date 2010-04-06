using System;
using System.IO;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Manual.CompareObject
{
    public abstract class BaseCompareObject
    {
        //Actual
        private readonly string _name;
        private long[] _creationTime;
        private bool[] _exists;
        private int _sourcePosition;

        //Meta
        private long[] _metaCreationTime;
        private bool[] _metaExists;
        private long[] _metaUpdated;

        //All       
        private MetaChangeType?[] _changeType;
        private FinalState?[] _finalState;
        private int[] _priority;
        private FolderCompareObject _parent;
        private bool _invalid;
        private string _newName;
        private LastKnownState?[] _lastKnownState;

        /// <summary>
        /// Initializes a <c>BaseCompareObject</c> given the name, the number of paths to synchronize, and the parent of it.
        /// </summary>
        /// <param name="name">A <see cref="string"/> containing the name.</param>
        /// <param name="numOfPaths">An <see cref="int"/> with the number of paths to synchronize.</param>
        /// <param name="parent">A <see cref="FolderCompareObject"/> which is the parent of this object. It can be null, in the case where this object is a <see cref="RootCompareObject"/>.</param>
        protected BaseCompareObject(string name, int numOfPaths, FolderCompareObject parent)
        {
            _name = name;
            _creationTime = new long[numOfPaths];
            _exists = new bool[numOfPaths];
            _finalState = new FinalState?[numOfPaths];
            _metaCreationTime = new long[numOfPaths];
            _metaExists = new bool[numOfPaths];
            _metaUpdated = new long[numOfPaths];
            _changeType = new MetaChangeType?[numOfPaths];
            _priority = new int[numOfPaths];
            _parent = parent;
            _invalid = false;
            _lastKnownState = new LastKnownState?[numOfPaths];
        }

        /// <summary>
        /// Gets the name of this <c>BaseCompareObject</c>.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }


        ///<summary>
        ///Gets or sets the <see cref="Array"/> of creation time.
        ///</summary>
        public long[] CreationTime
        {
            get { return _creationTime; }
            set { _creationTime = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Array"/> of metadata creation time.
        /// </summary>
        public long[] MetaCreationTime
        {
            get { return _metaCreationTime; }
            set { _metaCreationTime = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Array"/> of whether the actual file exists.
        /// </summary>
        public bool[] Exists
        {
            get { return _exists; }
            set { _exists = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Array"/> of whether the metadata file exists.
        /// </summary>
        public bool[] MetaExists
        {
            get { return _metaExists; }
            set { _metaExists = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Array"/> of <see cref="MetaChangeType"/>.
        /// </summary>
        public MetaChangeType?[] ChangeType
        {
            get { return _changeType; }
            set { _changeType = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Array"/> of <see cref="FinalState"/>.
        /// </summary>
        public FinalState?[] FinalState
        {
            get { return _finalState; }
            set { _finalState = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Array"/> of metadata updated time.
        /// </summary>
        public long[] MetaUpdated
        {
            get { return _metaUpdated; }
            set { _metaUpdated = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Array"/> of priority.
        /// </summary>
        public int[] Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="FolderCompareObject"/> parent.
        /// </summary>
        public FolderCompareObject Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        /// <summary>
        /// Gets or sets the invalid state.
        /// </summary>
        public bool Invalid
        {
            get { return _invalid; }
            set { _invalid = value; }
        }

        /// <summary>
        /// Gets or sets the new name.
        /// </summary>
        public string NewName
        {
            get { return _newName; }
            set { _newName = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Array"/> of <see cref="LastKnownState"/>.
        /// </summary>
        public LastKnownState?[] LastKnownState
        {
            get { return _lastKnownState; }
            set { _lastKnownState = value; }
        }

        /// <summary>
        /// Gets or sets the source position to be propagated to the rest.
        /// </summary>
        public int SourcePosition
        {
            get { return _sourcePosition; }
            set { _sourcePosition = value; }
        }

        /// <summary>
        /// This method returns the full parent path of a given <see cref="FileCompareObject"/> or <see cref="FolderCompareObject"/> based on the current state.
        /// </summary>
        /// <param name="index">The index determining which folder the <see cref="FileCompareObject"/> or <see cref="FolderCompareObject"/> belongs to.</param>
        /// <returns>A <see cref="string"/> with the full parent path.</returns>
        /// <remarks>When a file or folder has been renamed, or one or more of its ancestors has been renamed, the initial path to it is no longer valid. This method ensures that the most updated path will always be returned.</remarks>
        public string GetSmartParentPath(int index)
        {
            if (Parent == null)
                return "ROOT"; //Will throw exception in future

            RootCompareObject rco;
            if ((rco = Parent as RootCompareObject) != null)
                return rco.Paths[index];
            if (Parent.ChangeType[index] == MetaChangeType.Rename)
                return Path.Combine(Parent.GetSmartParentPath(index), Parent.NewName);
            return Path.Combine(Parent.GetSmartParentPath(index), Parent.FinalState[index] == Enum.FinalState.Renamed ? Parent.NewName : Parent.Name);
        }

    }
}