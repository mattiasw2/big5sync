using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Tagging.Exceptions;
using Syncless.Filters;
using Syncless.Helper;

namespace Syncless.Tagging
{
    public class Tag
    {
        private string _tagName;
        private long _lastUpdatedDate;
        private long _createdDate;
        private bool _isDeleted;
        private long _deletedDate;
        private List<TaggedPath> _pathList;
        private List<Filter> _filters;
        private long _filtersUpdatedDate;
        private TagConfig _config;

        public string TagName
        {
            get { return _tagName; }
            set { this._tagName = value; }
        }
        public long LastUpdatedDate
        {
            get { return _lastUpdatedDate; }
            set { _lastUpdatedDate = value; }
        }
        public long CreatedDate
        {
            get { return _createdDate; }
            set { _createdDate = value; }
        }
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { _isDeleted = value; }
        }
        public long DeletedDate
        {
            get { return _deletedDate; }
            set { _deletedDate = value; }
        }
        public List<string> FilteredPathListString
        {
            get
            {
                List<string> pathList = new List<string>();
                foreach (TaggedPath path in _pathList)
                {
                    if (!path.IsDeleted)
                    {
                        pathList.Add(path.PathName);
                    }
                }
                return pathList;
            }
        }
        public List<TaggedPath> FilteredPathList
        {
            get {
                List<TaggedPath> filteredList = new List<TaggedPath>();
                foreach (TaggedPath p in _pathList)
                {
                    if (!p.IsDeleted)
                    {
                        filteredList.Add(p);
                    }
                }
                return filteredList;
            }
            set { _pathList = value; }
        }
        public List<TaggedPath> UnfilteredPathList
        {
            get { return _pathList; }
            set { _pathList = value; }
        }
        public List<Filter> Filters
        {
            get { return _filters; }
            set { _filters = value; }
        }
        public long FiltersUpdatedDate
        {
            get { return _filtersUpdatedDate; }
            set { _filtersUpdatedDate = value; }
        }
        internal TagConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        public bool IsSeamless
        {
            get { return _config.IsSeamless; }
            set { _config.IsSeamless = value; }
        }
        public string ArchiveName
        {
            get { return _config.ArchiveFolderName; }
            set { _config.ArchiveFolderName = value; }
        }
        public int ArchiveCount
        {
            get { return _config.ArchiveCount; }
            set { _config.ArchiveCount = value; }
        }
        public bool Recycle
        {
            get { return _config.Recycle; }
            set { _config.Recycle = value; }
        }

        public Tag(string tagname, long created)
        {
            this._tagName = tagname;
            this._createdDate = created;
            this._lastUpdatedDate = created;
            this._isDeleted = false;
            this._deletedDate = 0;
            this._pathList = new List<TaggedPath>();
            this._filters = new List<Filter>();
            this._filtersUpdatedDate = created;
            this._config = new TagConfig();
        }

        public void Rename(string newname, long lastupdated)
        {
            _tagName = newname;
            _lastUpdatedDate = lastupdated;
        }

        public bool AddPath(string path, long created)
        {
            TaggedPath p = FindPath(path, false);
            if (p != null)
            {
                if (p.IsDeleted)
                {
                    _pathList.Remove(p);
                    TaggedPath taggedPath = new TaggedPath(path, created);
                    _lastUpdatedDate = TaggingHelper.GetCurrentTime();
                    _pathList.Add(taggedPath);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                TaggedPath taggedPath = new TaggedPath(path, created);
                _lastUpdatedDate = TaggingHelper.GetCurrentTime();
                _pathList.Add(taggedPath);
                return true;
            }
        }

        public bool AddPath(TaggedPath path)
        {
            TaggedPath p = FindPath(path.PathName, false);
            if (p != null)
            {
                if (p.IsDeleted)
                {
                    _pathList.Remove(p);
                    _pathList.Add(path);
                    _lastUpdatedDate = TaggingHelper.GetCurrentTime();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                _pathList.Add(path);
                _lastUpdatedDate = TaggingHelper.GetCurrentTime();
                return true;
            }
        }

        public void RenamePath(string oldPath, string newPath, long updated)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.PathName.StartsWith(PathHelper.FormatFolderPath(oldPath)))
                {
                    p.Replace(oldPath, newPath);
                }
                else if (p.PathName.Equals(PathHelper.FormatFolderPath(oldPath)))
                {
                    p.PathName = PathHelper.FormatFolderPath(newPath);
                }
                _lastUpdatedDate = updated;
            }
        }
        
        public bool RemovePath(string path, long lastupdated)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.PathName.Equals(PathHelper.FormatFolderPath(path)))
                {
                    if (p.IsDeleted)
                    {
                        return false;
                    }
                    else
                    {
                        p.Remove(lastupdated);
                        _lastUpdatedDate = TaggingHelper.GetCurrentTime();
                        return true;
                    }
                }
            }
            return false;
        }

        public bool RemovePath(TaggedPath path)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.PathName.Equals(PathHelper.FormatFolderPath(path.PathName)))
                {
                    if (p.IsDeleted)
                    {
                        return false;
                    }
                    else
                    {
                        long lastupdated = TaggingHelper.GetCurrentTime();
                        p.Remove(lastupdated);
                        _lastUpdatedDate = lastupdated;
                        return true;
                    }
                }
            }
            return false;
        }

        public void RemoveAllPaths()
        {
            foreach (TaggedPath p in _pathList)
            {
                p.IsDeleted = true;
                p.DeletedDate = TaggingHelper.GetCurrentTime();
            }
            _lastUpdatedDate = TaggingHelper.GetCurrentTime();
        }

        public bool ContainsIgnoreDeleted(string path)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.PathName.Equals(PathHelper.FormatFolderPath(path)))
                {
                    return true;
                }
            }
            return false;
        }

        //only return paths which are not set as deleted
        public bool Contains(string path)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.PathName.Equals(PathHelper.FormatFolderPath(path)))
                {
                    if (p.IsDeleted)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public string CreateTrailingPath(string path, bool isFolder)
        {
            string[] pathTokens = TaggingHelper.TrimEnd(path.Split('\\'));
            string logicalid = TaggingHelper.GetLogicalID(path);
            foreach (TaggedPath p in _pathList)
            {
                if (PathHelper.FormatFolderPath(path).StartsWith(p.PathName + "\\"))
                {
                    if (!PathHelper.FormatFolderPath(path).Equals(p.PathName))
                    {
                        string[] pTokens = TaggingHelper.TrimEnd(p.PathName.Split('\\'));
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

        public void AddFilter(Filter filter, long updated)
        {
            if (!_filters.Contains(filter))
            {
                _filters.Add(filter);
                _filtersUpdatedDate = updated;
                _lastUpdatedDate = TaggingHelper.GetCurrentTime();
            }
        }

        public void UpdateFilter(List<Filter> newFilterList)
        {
            CurrentTime current = new CurrentTime();
            _filters = newFilterList;
            _filtersUpdatedDate = current.CurrentTimeLong;
            _lastUpdatedDate = current.CurrentTimeLong;
        }

        public Filter RemoveFilter(Filter filter, long updated)
        {
            if (_filters.Contains(filter))
            {
                _filters.Remove(filter);
                _filtersUpdatedDate = updated;
                _lastUpdatedDate = TaggingHelper.GetCurrentTime();
                return filter;
            }
            else
            {
                return null;
            }
        }

        public TaggedPath FindPath(string path, bool filtered)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.PathName.Equals(PathHelper.FormatFolderPath(path)))
                {
                    if (filtered && p.IsDeleted)
                    {
                        return null;
                    }
                    return p;
                }
            }
            return null;
        }
        
        public TaggedPath FindPath(string path)
        {
            return FindPath(path, true);
        }

        public List<string> FindAncestors(string path)
        {
            List<string> ancestors = new List<string>();
            foreach (TaggedPath p in _pathList)
            {
                if (PathHelper.FormatFolderPath(path).StartsWith(p.PathName))
                {
                    if (!PathHelper.FormatFolderPath(path).Equals(p.PathName))
                    {
                        ancestors.Add(p.PathName);
                    }
                }
            }
            return ancestors;
        }

        public List<string> FindDescendants(string path)
        {
            List<string> descendants = new List<string>();
            foreach (TaggedPath p in _pathList)
            {
                if (p.PathName.StartsWith(PathHelper.FormatFolderPath(path)))
                {
                    if (!p.PathName.Equals(PathHelper.FormatFolderPath(path)))
                    {
                        descendants.Add(p.PathName);
                    }
                }
            }
            return descendants;
        }

        #region private implementations

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
