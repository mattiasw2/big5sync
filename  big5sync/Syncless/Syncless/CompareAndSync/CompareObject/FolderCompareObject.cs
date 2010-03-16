using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Enum;
using System.IO;
using System.Diagnostics;

namespace Syncless.CompareAndSync.CompareObject
{
    public class FolderCompareObject : BaseCompareObject
    {
        private Dictionary<string, BaseCompareObject> _contents;
        private bool _dirty;
        private string _metaName;
        private bool[] _useNewName;

        public FolderCompareObject(string name, int numOfPaths, FolderCompareObject parent)
            : base(name, numOfPaths, parent)
        {
            _contents = new Dictionary<string, BaseCompareObject>(StringComparer.OrdinalIgnoreCase);
            _useNewName = new bool[numOfPaths];
        }

        public void AddChild(BaseCompareObject child)
        {
            _contents.Add(child.Name, child);
        }

        public FileCompareObject GetSameCreationTime(long creationTime, int pos)
        {
            Dictionary<string, BaseCompareObject>.ValueCollection objects = _contents.Values;
            FileCompareObject f = null, result = null;
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

        //TODO: Shift this method to ComparerVisitor
        public FolderCompareObject GetRenamedFolder(string name, long creationTime, int pos)
        {
            Dictionary<string, BaseCompareObject>.ValueCollection objects = _contents.Values;
            FolderCompareObject f = null, result = null;
            int counter = 0;

            for (int i = 0; i < objects.Count; i++)
            {
                if ((f = objects.ElementAt(i) as FolderCompareObject) != null)
                {
                    if (f.MetaName != null && f.MetaName == name && f.Name != name && /*f.CreationTime[pos] == creationTime &&*/ f.ChangeType[pos] == MetaChangeType.New)
                    {
                        result = f;
                        counter++;
                    }
                }
            }

            return counter == 1 ? result : null;
        }

        //TODO: Shift this method to ComparerVisitor
        public FileCompareObject GetIdenticalFile(string name, string hash, long creationTime, int pos)
        {
            Dictionary<string, BaseCompareObject>.ValueCollection objects = _contents.Values;
            FileCompareObject f = null, result = null;
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

        public BaseCompareObject GetChild(string name)
        {
            BaseCompareObject child = null;

            if (_contents.TryGetValue(name, out child))
                return child;
            else
                return child;
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
            set { _dirty = value; }
        }

        public Dictionary<string, BaseCompareObject> Contents
        {
            get { return _contents; }
            set { _contents = value; }
        }

        public bool DoRename(int index)
        {
            return _useNewName[index];
        }

        public void UpdateRename(int posNewName)
        {
            for (int i = 0; i < _useNewName.Length; i++)
            {
                if (i != posNewName)
                    _useNewName[i] = true;
                else
                    _useNewName[i] = false;
            }
        }

    }
}
