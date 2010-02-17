using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging
{
    public abstract class Tag
    {
        protected string _tagName;

        public string TagName
        {
            get { return _tagName; }
            set { this._tagName = value; }
        }

        protected List<TaggedPath> _pathList;

        public List<TaggedPath> PathList
        {
            get { return _pathList; }
        }

        protected long _lastupdated;

        public long LastUpdated
        {
            get { return _lastupdated; }
            set { _lastupdated = value; }
        }

        protected long _created;

        public long Created
        {
            get { return _created; }
            set { _created = value; }
        }

        public Tag(string tagname, long created)
        {
            this._tagName = tagname;
            this._created = created;
            this._lastupdated = created;
            this._pathList = new List<TaggedPath>();
        }

        public bool AddPath(string path, long created)
        {
            if (!Contain(path))
            {
                TaggedPath taggedPath = new TaggedPath();
                taggedPath.Path = path;
                taggedPath.LogicalDriveId = GetLogicalID(path);
                taggedPath.Created = created;
                taggedPath.LastUpdated = created;
                _lastupdated = created;
                _pathList.Add(taggedPath);
                return true;
            }
            return false;
        }

        public bool RemovePath(string path, long lastupdated)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.Path.Equals(path))
                {
                    _pathList.Remove(p);
                    _lastupdated = lastupdated;
                    return true;
                }
            }
            return false;
        }

        public bool Contain(string path)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.Path.Equals(path))
                {
                    return true;
                }
            }
            return false;
        }

        #region private implementations
        protected string GetLogicalID(string path)
        {
            string[] tokens = path.Split('\\');
            return (tokens[0].TrimEnd(':'));
        }

        protected TaggedPath RetrieveTaggedPath(string path)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.Path.Equals(path))
                {
                    return p;
                }
            }
            return null;
        }

        private string GetCurrentTime()
        {
            DateTime current = DateTime.Now;
            string day = current.Day.ToString();
            string month = current.Month.ToString();
            string year = current.Year.ToString();
            string hour = current.Hour.ToString();
            string minute = current.Minute.ToString();
            string second = current.Second.ToString();
            string currenttime = string.Format("{0}{1}{2}{3}{4}{5}", day, month, year, hour, minute, second);
            return currenttime;
        }
        #endregion
    }
}
