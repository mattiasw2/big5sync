using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging.Exceptions;
using Syncless.Filters;
namespace Syncless.Tagging
{
    public class Tag
    {
        private string _tagName;
        private long _lastupdated;
        private long _created;
        private bool _isSeamless;
        private List<TaggedPath> _pathList;
        private List<Filter> _filters;
        
        public string TagName
        {
            get { return _tagName; }
            set { this._tagName = value; }
        }
        public List<Filter> Filters
        {
            get { return _filters; }
        }
        public List<TaggedPath> PathList
        {
            get { return _pathList; }
        }
        public List<string> PathStringList
        {
            get
            {
                List<string> pathList = new List<string>();
                foreach (TaggedPath path in _pathList)
                {
                    pathList.Add(path.Path);
                }
                return pathList;
            }
        }
        public long LastUpdated
        {
            get { return _lastupdated; }
            set { _lastupdated = value; }
        }
        public long Created
        {
            get { return _created; }
            set { _created = value; }
        }
        public bool IsSeamless
        {
            get { return _isSeamless; }
            set { _isSeamless = value; }
        }

        public Tag(string tagname, long created)
        {
            this._tagName = tagname;
            this._created = created;
            this._lastupdated = created;
            this._isSeamless = true;
            this._pathList = new List<TaggedPath>();
            this._filters = new List<Filter>();
        }

        public bool AddPath(string path, long created)
        {
            if (!Contains(path))
            {
                TaggedPath taggedPath = new TaggedPath();
                taggedPath.Path = path;
                taggedPath.LogicalDriveId = TaggingHelper.GetLogicalID(path);
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

        public bool Contains(string path)
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

        public void Rename(string oldPath, string newPath)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.Path.StartsWith(oldPath))
                {
                    p.Replace(oldPath, newPath);
                }
                else if (p.Path.Equals(oldPath))
                {
                    p.Path = newPath;
                }
            }
        }

        public string FindMatchedParentDirectory(string path, bool isFolder)
        {
            string[] pathTokens = TaggingHelper.TrimEnd(path.Split('\\'));
            string logicalid = TaggingHelper.GetLogicalID(path);
            foreach (TaggedPath p in _pathList)
            {
                if (path.StartsWith(p.Path))
                {
                    if (!path.Equals(p.Path))
                    {
                        string[] pTokens = TaggingHelper.TrimEnd(p.Path.Split('\\'));
                        int trailingIndex = TaggingHelper.Match(pathTokens, pTokens);
                        if (trailingIndex > 0)
                        {
                            return TaggingHelper.CreatePath(trailingIndex, pathTokens, isFolder);
                        }
                    }
                }
            }
            return null;
        }

        public void AddFilter(Filter filter)
        {
            if (!_filters.Contains(filter))
            {
                _filters.Add(filter);
            }
        }

        public Filter RemoveFilter(Filter filter)
        {
            if (_filters.Contains(filter))
            {
                _filters.Remove(filter);
                return filter;
            }
            else
            {
                return null;
            }
        }

        #region private implementations
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

        #region Deprecated
        /// <summary>
        /// Deprecated - Find if pathTokens starts with pTokens (replaced by StartsWith method of String class)
        /// </summary>
        /// <param name="pathTokens"></param>
        /// <param name="pTokens"></param>
        /// <returns></returns>
        protected bool CheckMatch(string[] pathTokens, string[] pTokens)
        {
            for (int i = 0; i < pTokens.Length; i++)
            {
                if (!pTokens[i].Equals(pathTokens[i]))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #endregion
    }
}
