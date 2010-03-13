using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Enum;
using System.IO;

namespace Syncless.CompareAndSync.CompareObject
{
    public class FolderCompareObject : BaseCompareObject
    {
        private Dictionary<string, BaseCompareObject> _contents;
        //private List<string> _possibleNewNames;
        private bool _dirty;
        private string _metaName;
        private string[] _newNames;

        public FolderCompareObject(string name, int numOfPaths, FolderCompareObject parent)
            : base(name, numOfPaths, parent)
        {
            _contents = new Dictionary<string, BaseCompareObject>(StringComparer.OrdinalIgnoreCase);
            //_possibleNewNames = new List<string>();
            _newNames = new string[numOfPaths];
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
                    if (objects.ElementAt(i) is FileCompareObject)
                    {
                        f = (FileCompareObject)objects.ElementAt(i);
                        if (f.CreationTime[pos] == creationTime)
                        {
                            result = (FileCompareObject)objects.ElementAt(i);
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
                    if (f.MetaName != null && f.MetaName == name && f.Name != name && f.ChangeType[pos] == MetaChangeType.New)
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
                        if (f.Exists[pos] && f.Name != name && f.CreationTime[pos] == creationTime && f.Hash[pos] == hash)
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

        public string[] NewNames
        {
            get { return _newNames; }
            set { _newNames = value; }
        }

        public override string GetFullParentPath(int index)
        {
            RootCompareObject root = null;
            if ((root = Parent as RootCompareObject) != null)
                return root.Paths[index];
            else
                return Path.Combine(Parent.GetFullParentPath(index), string.IsNullOrEmpty(Parent.NewNames[index]) ? Parent.Name : Parent.NewNames[index]);
        }
    }
}
