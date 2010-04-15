/*
 * 
 * Author: Soh Yuan Chin
 * 
 */

using System;
using System.IO;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Manual.CompareObject
{
    /// <summary>
    /// Abstract class <c>BaseCompareObject</c>.
    /// </summary>
    /// <remarks>
    /// A BaseCompareObject stores all information of a file system object at a particular directory by name.
    /// For example, C:\FolderToSync\1.txt and D:\Folder2Sync\1.txt will both be the same BaseCompareObject with name "1.txt"
    /// if C:\FolderToSync and D:\Folder2Sync are to be synchronized. Arrays of various types are then used to keep information
    /// of each individual file.
    /// </remarks>
    public abstract class BaseCompareObject
    {
        // Actual
        private readonly string _name; // Name of file system object (file or folder).
        private long[] _creationTimeUtc; // Array to store the creation time in UTC of each actual object.
        private bool[] _exists; // Array to state whether the file system object actually exists.
        private int _sourcePosition; // Used to indicate the position of source, to be propagated to all the other positions.

        // Meta
        private long[] _metaCreationTimeUtc; // Array to store the creation time in UTC from the metadata.
        private bool[] _metaExists; // Array to state whether metadata exists for the given file system object.
        private long[] _metaUpdated; // Array that stores when the metadata for the file system object was last updated.

        // All       
        private MetaChangeType?[] _changeType; // Array to store the metadata change type after being populated with actual and meta information.
        private FinalState?[] _finalState; // Array to store the final state of each file system object after synchronization.
        private int[] _priority; // Array to store the priority of each file system object. Highest priority indicates source position.
        private FolderCompareObject _parent; // Parent of the file system object.
        private bool _invalid; // Specifies if this node is invalid.
        private string _newName; // Stores the new name of the file system object, in the case of rename.
        private LastKnownState?[] _lastKnownState; // Stores the last known state, eg. delete, if the file system object is no longer found.

        /// <summary>
        /// Initializes a <c>BaseCompareObject</c> given the name, the number of paths to synchronize, and the parent of it.
        /// </summary>
        /// <param name="name">A <see cref="string"/> containing the name.</param>
        /// <param name="numOfPaths">An <see cref="int"/> with the number of paths to synchronize.</param>
        /// <param name="parent">A <see cref="FolderCompareObject"/> which is the parent of this object. It can be null, in the case where this object is a <see cref="RootCompareObject"/>.</param>
        protected BaseCompareObject(string name, int numOfPaths, FolderCompareObject parent)
        {
            _name = name;
            _creationTimeUtc = new long[numOfPaths];
            _exists = new bool[numOfPaths];
            _finalState = new FinalState?[numOfPaths];
            _metaCreationTimeUtc = new long[numOfPaths];
            _metaExists = new bool[numOfPaths];
            _metaUpdated = new long[numOfPaths];
            _changeType = new MetaChangeType?[numOfPaths];
            _priority = new int[numOfPaths];
            _parent = parent;
            _invalid = false;
            _lastKnownState = new LastKnownState?[numOfPaths];
        }

        #region Properties

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
        public long[] CreationTimeUtc
        {
            get { return _creationTimeUtc; }
            set { _creationTimeUtc = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Array"/> of metadata creation time.
        /// </summary>
        public long[] MetaCreationTimeUtc
        {
            get { return _metaCreationTimeUtc; }
            set { _metaCreationTimeUtc = value; }
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

        #endregion

        #region Methods

        /// <summary>
        /// This method returns the full parent path of a given <see cref="FileCompareObject"/> or <see cref="FolderCompareObject"/> based on the current state.
        /// </summary>
        /// <param name="index">The index determining which folder the <see cref="FileCompareObject"/> or <see cref="FolderCompareObject"/> belongs to.</param>
        /// <returns>A <see cref="string"/> with the full parent path.</returns>
        /// <remarks>When a file or folder has been renamed, or one or more of its ancestors has been renamed, the initial path to it is no longer valid. This method ensures that the most updated path will always be returned.</remarks>
        public string GetSmartParentPath(int index)
        {
            if (Parent == null)
                return "ROOT"; // Will throw exception in future

            RootCompareObject rco;
            if ((rco = Parent as RootCompareObject) != null)
                return rco.Paths[index]; // If the parent is already a root compare object, simply return it.
            if (Parent.ChangeType[index] == MetaChangeType.Rename)
                return Path.Combine(Parent.GetSmartParentPath(index), Parent.NewName); // If the parent has a MetaChangeType of rename, recursively call it and use the parent's new name.
            return Path.Combine(Parent.GetSmartParentPath(index), Parent.FinalState[index] == Enum.FinalState.Renamed ? Parent.NewName : Parent.Name); // If the parent's final state is renamed, use the new name, else use the name.
        }

        #endregion

    }
}