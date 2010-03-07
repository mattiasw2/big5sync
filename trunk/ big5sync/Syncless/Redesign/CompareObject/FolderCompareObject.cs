using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Redesign
{
    public class FolderCompareObject : CompareObject
    {
        List<CompareObject> _contents;

        public FolderCompareObject(string origin, DirectoryInfo dir)
            : base(origin)
        {
            base._fullName = dir.FullName;
            base._name = dir.Name;
            base._creationTime = dir.CreationTime.Ticks;
            FillContents(dir);
        }

        public FolderCompareObject(string origin, string fullName, string name, long creationTime)
            : base(origin, fullName, name, creationTime)
        {
            FillContents();
        }

        public List<CompareObject> Contents
        {
            get { return _contents; }
            set { _contents = value; }
        }

        public bool IsOrigin
        {
            get { return base._origin.Equals(base._fullName); }
        }

        public bool IsEmpty
        {
            get { return _contents.Count() == 0; }
        }

        public void FillContents()
        {
            FillContents(new DirectoryInfo(base._fullName));
        }

        private void FillContents(DirectoryInfo dir)
        {
            _contents = new List<CompareObject>();
            DirectoryInfo[] folders = dir.GetDirectories();
            FileInfo[] files = dir.GetFiles();

            foreach (DirectoryInfo folder in folders)
            {
                _contents.Add(new FolderCompareObject(base._origin, folder));
            }

            foreach (FileInfo file in files)
            {
                _contents.Add(new FileCompareObject(base._origin, file));
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Folder: " + base._fullName);

            foreach (CompareObject item in _contents)
            {
                sb.Append("\n"+item.ToString());
            }

            return sb.ToString();
        }

    }
}
