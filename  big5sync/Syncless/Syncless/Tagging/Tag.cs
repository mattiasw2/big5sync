using System.Collections.Generic;
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
            set { _tagName = value; }
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
        public List<Filter> ReadOnlyFilters
        {
            get
            {
                List<Filter> readOnlyFilters = new List<Filter>();
                readOnlyFilters.AddRange(Filters);
                return readOnlyFilters;
            }
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

        public void Remove(long updated)
        {
            _isDeleted = true;
            _deletedDate = updated;
            _lastUpdatedDate = updated;
            RemoveAllPaths();
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

        public int RenamePath(string oldPath, string newPath, long updated)
        {
            List<TaggedPath> newTaggedPathList = new List<TaggedPath>();
            foreach (TaggedPath p in FilteredPathList)
            {
                if (PathHelper.StartsWithIgnoreCase(p.PathName, oldPath))
                {
                    p.Remove(updated);
                    string combinedPath = p.PathName.Replace(oldPath, newPath);
                    TaggedPath newTaggedPath = new TaggedPath(combinedPath, updated);
                    if (!newTaggedPathList.Contains(newTaggedPath))
                    {
                        newTaggedPathList.Add(newTaggedPath);
                    }
                }
                //else if (PathHelper.EqualsIgnoreCase(p.PathName, oldPath))
                //{
                //    p.Remove(updated);
                //    TaggedPath newTaggedPath = new TaggedPath(newPath, updated);
                //    if (!newTaggedPathList.Contains(newTaggedPath))
                //    {
                //        newTaggedPathList.Add(newTaggedPath);
                //    }
                //}
            }
            if (newTaggedPathList.Count > 0)
            {
                foreach (TaggedPath toAdd in newTaggedPathList)
                {
                    _pathList.Add(toAdd);
                }
                _lastUpdatedDate = updated;
            }
            return newTaggedPathList.Count;
        }
        
        public bool RemovePath(string path, long lastupdated)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (PathHelper.EqualsIgnoreCase(p.PathName, path))
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
                if (PathHelper.EqualsIgnoreCase(p.PathName, path.PathName))
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
            long deletedDate = TaggingHelper.GetCurrentTime();
            foreach (TaggedPath p in _pathList)
            {
                p.Remove(deletedDate);
            }
            _lastUpdatedDate = deletedDate;
        }

        /// <summary>
        /// Return true if this tag contains a path whose parent is the given path
        /// </summary>
        /// <param name="path">The path to find the children path</param>
        /// <returns>True if tag contains child path, else false</returns>
        public bool ContainsParent(string path)
        {
            foreach (TaggedPath p in FilteredPathList)
            {
                if (PathHelper.StartsWithIgnoreCase(p.PathName, path))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Return true if this tag contains a path whose parent is the given path, regardless of whether
        /// the path is set as deleted or not
        /// </summary>
        /// <param name="path">The path to find the children path</param>
        /// <returns>True if tag contains child path, else false</returns>
        public bool ContainsParentIgnoreDeleted(string path)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (PathHelper.StartsWithIgnoreCase(p.PathName, path))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if this tag contains the given path regardless of whether it is set as deleted or not
        /// </summary>
        /// <param name="path">The path to find</param>
        /// <returns>True if tag contains the given path</returns>
        public bool ContainsIgnoreDeleted(string path)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (PathHelper.EqualsIgnoreCase(p.PathName, path))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if this tag contains the given path, only if it is not set as deleted
        /// </summary>
        /// <param name="path">The path to find</param>
        /// <returns>True if tag contains the given path</returns>
        public bool Contains(string path)
        {
            foreach (TaggedPath p in FilteredPathList)
            {
                if (PathHelper.EqualsIgnoreCase(p.PathName, path))
                {
                    return true;
                    //if (p.IsDeleted)
                    //{
                    //    return false;
                    //}
                    //else
                    //{
                    //    return true;
                    //}
                }
            }
            return false;
        }

        /// <summary>
        /// Extract the trailing path from the path tagged to this tag
        /// Example: tag contains D:\A\B\C
        ///          Given the path is D:\A\B\C\E\F\G\
        ///          Return E\F\G\
        /// </summary>
        /// <param name="path">The child path</param>
        /// <param name="isFolder">Indicates whether the path is a folder path or file path</param>
        /// <returns>The trailing end of the given path</returns>
        public string CreateTrailingPath(string path, bool isFolder)
        {
            string[] pathTokens = path.Trim().Split('\\');
            string logicalid = TaggingHelper.GetLogicalID(path);
            foreach (TaggedPath p in FilteredPathList)
            {
                if (PathHelper.StartsWithIgnoreCase(path, p.PathName))
                {
                    if (!PathHelper.EqualsIgnoreCase(path, p.PathName))
                    {
                        string[] pTokens = p.PathName.Trim().Split('\\');
                        int trailingIndex = TaggingHelper.Match(pathTokens, pTokens);
                        if (trailingIndex > 0)
                        {
                            return TaggingHelper.CreatePath(trailingIndex, pathTokens);
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
            List<Filter> updatedList = new List<Filter>();
            foreach (Filter filter in newFilterList)
            {
                if (!updatedList.Contains(filter))
                {
                    updatedList.Add(filter);
                }
            }
            CurrentTime current = new CurrentTime();
            _filters = updatedList;
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
                if (PathHelper.EqualsIgnoreCase(p.PathName, path))
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

        /// <summary>
        /// Find the list of paths which are the ancestors of the given path
        /// </summary>
        /// <param name="path">The path whose ancestors are to be retrieved</param>
        /// <returns>The list of ancestors' paths</returns>
        public List<string> FindAncestors(string path)
        {
            List<string> ancestors = new List<string>();
            foreach (TaggedPath p in FilteredPathList)
            {
                if (PathHelper.StartsWithIgnoreCase(path, p.PathName))
                {
                    if (!PathHelper.EqualsIgnoreCase(path, p.PathName))
                    {
                        if (!PathHelper.ContainsIgnoreCase(ancestors, p.PathName))
                        {
                            ancestors.Add(p.PathName);
                        }
                    }
                }
            }
            return ancestors;
        }

        /// <summary>
        /// Find the list of paths which are the descendants of the given path
        /// </summary>
        /// <param name="path">The path whose descendants are to be retrieved</param>
        /// <returns>The list of descendants' paths</returns>
        public List<string> FindDescendants(string path)
        {
            List<string> descendants = new List<string>();
            foreach (TaggedPath p in FilteredPathList)
            {
                if (PathHelper.StartsWithIgnoreCase(p.PathName, path))
                {
                    if (!PathHelper.EqualsIgnoreCase(p.PathName, path))
                    {
                        if (!PathHelper.ContainsIgnoreCase(descendants, p.PathName))
                        {
                            descendants.Add(p.PathName);
                        }
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
