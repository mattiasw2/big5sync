using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompareAndSync.CompareObject
{
    public class FolderCompareObject : BaseCompareObject
    {
        private Dictionary<string, BaseCompareObject> _contents;

        public FolderCompareObject(string name, int numOfPaths, FolderCompareObject parent)
            : base(name, numOfPaths, parent)
        {
            _contents = new Dictionary<string, BaseCompareObject>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddChild(BaseCompareObject child)
        {
            _contents.Add(child.Name, child);

            /*
            if (child is FileCompareObject)
            {
                FileCompareObject c = (FileCompareObject)child;
                List<string> listOfNames = null;

                if (_fileHashToName.TryGetValue(c.Hash, out listOfNames))
                {

                }
            }*/

        }

        public FileCompareObject GetIdenticalFile(string hash, long creationTime)
        {
            Dictionary<string, BaseCompareObject>.ValueCollection objects = _contents.Values;
            FileCompareObject f = null, result = null;
            int counter = 0, indexOfHash = -1, indexOfCreationTime = -1;

            for (int i = 0; i < objects.Count; i++)
            {
                {
                    if (objects.ElementAt(i) is FileCompareObject)
                    {
                        f = (FileCompareObject)objects.ElementAt(i);
                        indexOfHash = Array.IndexOf<string>(f.Hash, hash);
                        indexOfCreationTime = Array.IndexOf<long>(f.CreationTime, creationTime);
                        if (indexOfHash > -1 && indexOfCreationTime > -1 && indexOfHash == indexOfCreationTime && f.Exists[indexOfHash])
                        {
                            result = (FileCompareObject)objects.ElementAt(i);
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

        public bool ContainsChild(string name)
        {
            return _contents.ContainsKey(name);
        }

        public Dictionary<string, BaseCompareObject> Contents
        {
            get { return _contents; }
            set { _contents = value; }
        }
    }
}
