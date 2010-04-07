using System;
using System.Collections.Generic;
using System.Linq;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Manual.CompareObject
{
    /// <summary>
    /// The <c>FolderCompareObject</c> class stores all information related to a folder node .
    /// </summary>
    public class FolderCompareObject : BaseCompareObject
    {
        private Dictionary<string, BaseCompareObject> _contents;
        private bool _dirty;
        private string _metaName;
        private bool[] _useNewName;

        /// <summary>
        /// Initializes a <c>FolderCompareObject</c> given the name of the file, the number of paths to synchronize, and the parent of this file.
        /// </summary>
        /// <param name="name">A <see cref="string"/> containing the name of this <c>FolderCompareObject</c>.</param>
        /// <param name="numOfPaths">An <see cref="int"/> containing the number of paths to synchronize.</param>
        /// <param name="parent">The <see cref="FolderCompareObject"/> which is the parent of this <c>FolderCompareObject</c>.</param>
        public FolderCompareObject(string name, int numOfPaths, FolderCompareObject parent)
            : base(name, numOfPaths, parent)
        {
            _contents = new Dictionary<string, BaseCompareObject>(StringComparer.OrdinalIgnoreCase);
            _useNewName = new bool[numOfPaths];
        }

        /// <summary>
        /// Adds a child to the <c>FolderCompareObject</c>.
        /// </summary>
        /// <param name="child"><see cref="BaseCompareObject"/> to add to to this <c>FolderCompareObject</c>.</param>
        public void AddChild(BaseCompareObject child)
        {
            _contents.Add(child.Name, child);
        }

        /// <summary>
        /// Gets another <see cref="FileCompareObject"/> with the same creation time as the one passed in.
        /// </summary>
        /// <param name="creationTime">A <see cref="long"/> with the creation time to search for.</param>
        /// <param name="pos"></param>
        /// <returns><c>The FileCompareObject with the same creation time as that passed in.</c></returns>
        public FileCompareObject GetSameCreationTime(long creationTime, int pos)
        {
            Dictionary<string, BaseCompareObject>.ValueCollection objects = _contents.Values;
            FileCompareObject f, result = null;
            int counter = 0;

            for (int i = 0; i < objects.Count; i++)
            {
                {
                    if ((f = objects.ElementAt(i) as FileCompareObject) != null)
                    {
                        if (f.CreationTime[pos] == creationTime)
                        {
                            result = f;
                            counter++;
                        }
                    }
                }
            }

            return counter == 1 ? result : null;
        }

        /// <summary>
        /// Finds the possible folder that the previous folder was renamed to.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <param name="creationTime">The creation time to match.</param>
        /// <param name="pos"></param>
        /// <param name="renameCount"></param>
        /// <returns></returns>
        public FolderCompareObject GetRenamedFolder(string name, out int renameCount)
        {
            Dictionary<string, BaseCompareObject>.ValueCollection objects = _contents.Values;
            FolderCompareObject f, result = null;
            int counter = 0;

            for (int i = 0; i < objects.Count; i++)
            {
                if ((f = objects.ElementAt(i) as FolderCompareObject) != null)
                {
                    if (f.MetaName != null && f.MetaName == name && f.Name != name && f.MetaName != f.Name)
                    {
                        result = f;
                        counter++;
                    }
                }
            }

            renameCount = counter;

            return counter == 1 ? result : null;
        }

        /// <summary>
        /// Finds an identical file with the given hash and name.
        /// </summary>
        /// <param name="name">The name to match.</param>
        /// <param name="hash">The hash to match.</param>
        /// <param name="creationTime">The creation time to match.</param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public FileCompareObject GetIdenticalFile(string name, string hash, long creationTime, int pos)
        {
            Dictionary<string, BaseCompareObject>.ValueCollection objects = _contents.Values;
            FileCompareObject f, result = null;
            int counter = 0;

            for (int i = 0; i < objects.Count; i++)
            {
                {
                    if ((f = objects.ElementAt(i) as FileCompareObject) != null)
                    {
                        if (f.Name != name && f.CreationTime[pos] == creationTime && f.Hash[pos] == hash && f.ChangeType[pos] == MetaChangeType.New)
                        {
                            result = f;
                            counter++;
                        }
                    }
                }
            }

            return counter == 1 ? result : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public BaseCompareObject GetChild(string name)
        {
            BaseCompareObject child;

            if (_contents.TryGetValue(name, out child))
                return child;
            return child;
        }

        public bool RemoveChild(string name)
        {
            return _contents.Remove(name);
        }

        public string MetaName
        {
            get { return _metaName; }
            set { _metaName = value; }
        }

        public bool ContainsChild(string name)
        {
            return _contents.ContainsKey(name);
        }

        public bool Dirty
        {
            get { return _dirty; }
            set
            {

                if (Parent != null && value)
                    Parent.Dirty = true;
                _dirty = value;
            }
        }

        public Dictionary<string, BaseCompareObject> Contents
        {
            get { return _contents; }
            set { _contents = value; }
        }

    }
}