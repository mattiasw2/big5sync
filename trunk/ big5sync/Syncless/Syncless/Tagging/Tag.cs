using System.Collections.Generic;
using Syncless.Filters;
using Syncless.Helper;

namespace Syncless.Tagging
{
    /// <summary>
    /// Tag class represents a container for a list of <see cref="TaggedPath">TaggedPath</see> objects.
    /// Each Tag object has its properties that uniquely identifies itself from other Tag objects.
    /// </summary>
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

        /// <summary>
        /// Gets or sets the name of the tag
        /// </summary>
        public string TagName
        {
            get { return _tagName; }
            set { _tagName = value; }
        }

        /// <summary>
        /// Gets or sets the last updated date of the tag
        /// </summary>
        public long LastUpdatedDate
        {
            get { return _lastUpdatedDate; }
            set { _lastUpdatedDate = value; }
        }

        /// <summary>
        /// Gets or sets the created date of the tag
        /// </summary>
        public long CreatedDate
        {
            get { return _createdDate; }
            set { _createdDate = value; }
        }

        /// <summary>
        /// Gets or sets the boolean value that represents whether the tag is deleted
        /// </summary>
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { _isDeleted = value; }
        }

        /// <summary>
        /// Gets or sets the deleted date of the tag
        /// </summary>
        public long DeletedDate
        {
            get { return _deletedDate; }
            set { _deletedDate = value; }
        }

        /// <summary>
        /// Gets a clone of the list of full path name of tagged paths whose <see cref="IsDeleted">IsDeleted
        /// </see>property is set to false
        /// </summary>
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

        /// <summary>
        /// Gets a clone of the list of tagged paths whose <see cref="IsDeleted">IsDeleted</see> property
        /// is set to false. Sets the list of tagged paths.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the list of tagged paths 
        /// </summary>
        public List<TaggedPath> UnfilteredPathList
        {
            get { return _pathList; }
            set { _pathList = value; }
        }

        /// <summary>
        /// Gets or sets the list of filters of the tag
        /// </summary>
        public List<Filter> Filters
        {
            get { return _filters; }
            set { _filters = value; }
        }

        /// <summary>
        /// Gets a clone of the list of filters of the tag
        /// </summary>
        public List<Filter> ReadOnlyFilters
        {
            get
            {
                List<Filter> readOnlyFilters = new List<Filter>();
                readOnlyFilters.AddRange(Filters);
                return readOnlyFilters;
            }
        }

        /// <summary>
        /// Gets or sets the updated date of the filters of the tag
        /// </summary>
        public long FiltersUpdatedDate
        {
            get { return _filtersUpdatedDate; }
            set { _filtersUpdatedDate = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="TagConfig">TagConfig</see> object of the tag
        /// </summary>
        internal TagConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        /// <summary>
        /// Gets or sets the boolean value which represents whether the tag is in seamless mode
        /// </summary>
        public bool IsSeamless
        {
            get { return _config.IsSeamless; }
            set { _config.IsSeamless = value; }
        }
        
        /// <summary>
        /// Creates a new Tag object
        /// </summary>
        /// <param name="tagname">The string value that represents the name to be given to the tag</param>
        /// <param name="created">The long value that represents the created date of the tag</param>
        /// <remarks>The last updated date of the tag is set to the created date that is passed as
        /// parameter. The boolean value that represents whether the tag is deleted is set to false by
        /// default. The deleted date of the tag is set to 0 by default. The updated date of filter
        /// is set to the created date that is passed as parameter. New instance of a list of tagged paths
        /// is created. New instance of a list of filters is created. New instance of tag config is
        /// created.</remarks>
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

        /// <summary>
        /// Sets the name of the tag to the new name that is passed as parameter
        /// </summary>
        /// <param name="newname">The string value that represents the new name to be given to the tag</param>
        /// <param name="lastupdated">The long value that represents the last updated date to be given
        /// to the tag</param>
        public void Rename(string newname, long lastupdated)
        {
            _tagName = newname;
            _lastUpdatedDate = lastupdated;
        }

        /// <summary>
        /// Sets the boolean value that represents whether the tag is deleted to true
        /// </summary>
        /// <param name="updated">The long value that represents the deleted date of the tag</param>
        /// <remarks>The last updated date is set to the deleted date that is passed as parameter.
        /// The boolean value that represents whether the tagged path is deleted for each of the list
        /// of tagged paths is set to true.</remarks>
        public void Remove(long updated)
        {
            _isDeleted = true;
            _deletedDate = updated;
            _lastUpdatedDate = updated;
            RemoveAllPaths();
        }

        /// <summary>
        /// Creates and adds to the tag a tagged path that represents the full path name that is passed
        /// as parameter
        /// </summary>
        /// <param name="path">The string value that represents the full path name to be added</param>
        /// <param name="created">The long value that represents the created date of the tagged path</param>
        /// <returns>true if the tagged path is added to the tag; otherwise, false</returns>
        /// <remarks>If there already exists a tagged path which represents the full path name that is
        /// passed as parameter, if the boolean value that represents whether the tagged path is deleted is
        /// true, the existing tagged path will be removed from the list of tagged paths. A new tagged path
        /// will be created and be added to the list of tagged paths.</remarks>
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

        /// <summary>
        /// Adds the tagged path to the tag
        /// </summary>
        /// <param name="path">The <see cref="TaggedPath">TaggedPath</see> that is to be added to 
        /// the tag</param>
        /// <returns>true if the tagged path is added to the tag; otherwise, false</returns>
        /// <remarks>If there already exists a tagged path whose full path name is the same as that of 
        /// the tagged path that is passed as parameter, if the boolean value that represents whether 
        /// the tagged path is deleted is true, the existing tagged path will be removed from the 
        /// list of tagged paths. The tagged path that is passed as parameter will be added to the list of 
        /// tagged paths.</remarks>
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

        /// <summary>
        /// Sets part of the full path name, contained by one or more of the tagged paths in the list 
        /// of tagged paths, represented by the old path that is passed as parameter to the new path 
        /// that is passed as parameter
        /// </summary>
        /// <param name="oldPath">The string value that represents the old path which is part of the 
        /// full path name of the tagged path</param>
        /// <param name="newPath">The string value that represents the new path which is to replace
        /// part of the full path name of the tagged path represented by the old path that is 
        /// passed as parameter</param>
        /// <param name="updated">The long value that represents the updated date of the tag and the
        /// tagged path whose full path name is renamed</param>
        /// <returns>The number of tagged paths whose full path name represented by the old path is
        /// replaced by the new path</returns>
        /// <remarks>If the old path that is passed as parameter is not the full path name but part of 
        /// the full path name, only the matching part of the path name will be replaced with the
        /// new path that is passed as parameter</remarks>
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
        
        /// <summary>
        /// Sets the boolean value that represents whether the tagged path, that represents the path that is
        /// passed as parameter, is deleted to true
        /// </summary>
        /// <param name="path">The string value that represents the full path name of the tagged path to be
        /// set as deleted</param>
        /// <param name="lastupdated">The long value that represents the last updated date of the tagged
        /// path</param>
        /// <returns>true if some tagged paths are set as deleted; otherwise, false</returns>
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

        /// <summary>
        /// Sets the boolean value that represents whether the tagged path is deleted to true
        /// </summary>
        /// <param name="path">The <see cref="TaggedPath">TaggedPath</see> to be set as deleted</param>
        /// <returns>true if some tagged paths are set as deleted; otherwise, false</returns>
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

        /// <summary>
        /// Sets the boolean value that represents whether a tagged path is deleted to true for all tagged paths
        /// </summary>
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
        /// Checks if any tagged path in the filtered list of tagged paths contains a full path name 
        /// whose parent path is represented by the path that is passed as paramater
        /// </summary>
        /// <param name="path">The string value that represents the path to be used to find the 
        /// children path</param>
        /// <returns>true if any tagged path in the filtered list of tagged paths contains a full 
        /// path name whose parent path is the path that is passed as parameter; otherwise, false</returns>
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
        /// Checks if any tagged path in the list of tagged paths contains a full path name 
        /// whose parent path is represented by the path that is passed as paramater
        /// </summary>
        /// <param name="path">The string value that represents the path to be used to find the 
        /// children path</param>
        /// <returns>true if any tagged path in the list of tagged paths contains a full 
        /// path name whose parent path is the path that is passed as parameter; otherwise, false</returns>
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
        /// Checks if the list of tagged paths contains a tagged path that represents the path that is passed
        /// as parameter
        /// </summary>
        /// <param name="path">The string value that represents the path to be used to check if any
        /// tagged path contains this path</param>
        /// <returns>true if any tagged path in the list of tagged paths contains a full path name that is
        /// the same as the path that is passed as parameter</returns>
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
        /// Checks if the filtered list of tagged paths contains a tagged path that represents the 
        /// path that is passed as parameter
        /// </summary>
        /// <param name="path">The string value that represents the path to be used to check if any
        /// tagged path contains this path</param>
        /// <returns>true if any tagged path in the filtered list of tagged paths contains a full 
        /// path name that is the same as the path that is passed as parameter</returns>
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
        /// Extracts the trailing path from the path that is passed as parameter, whose parent path is the 
        /// full path name that is represented by some tagged paths in the filtered list of tagged paths
        /// </summary>
        /// <param name="path">The string value that represents the path whose parent path is represented
        /// by some of the tagged paths</param>
        /// <param name="isFolder">The boolean value that represents whether the path that is passed as 
        /// parametere is a folder path or file path</param>
        /// <returns>the trailing end of the given path if its parent path is represented by some tagged
        /// paths in the filtered list of tagged paths; otherwise, null</returns>
        /// <remarks>Example: tag contains D:\A\B\C
        ///          Given the path is D:\A\B\C\E\F\G\
        ///          Return E\F\G\</remarks>
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

        /// <summary>
        /// Adds a filter to the list of filters
        /// </summary>
        /// <param name="filter">The <see cref="Filter">Filter</see> object that represents the filter 
        /// to be added</param>
        /// <param name="updated">The long value that represents the updated date of the filter list</param>
        public void AddFilter(Filter filter, long updated)
        {
            if (!_filters.Contains(filter))
            {
                _filters.Add(filter);
                _filtersUpdatedDate = updated;
                _lastUpdatedDate = TaggingHelper.GetCurrentTime();
            }
        }

        /// <summary>
        /// Adds the filters in the new filter list that is passed as parameter to the existing list of filters
        /// </summary>
        /// <param name="newFilterList">The list of filters containing filters to be added to the existing
        /// list of filters</param>
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

        /// <summary>
        /// Removes the filter that is passed as parameter from the existing list of filters
        /// </summary>
        /// <param name="filter">The <see cref="Filter">Filter</see> object that represents the fitler
        /// to be removed</param>
        /// <param name="updated">The long value that represents the updated date of the filter list</param>
        /// <returns>the filter that is removed if it exists in the list of filters; otherwise, null</returns>
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

        /// <summary>
        /// Finds the tagged path that represents the path that is passed as parameter from the list of
        /// tagged paths
        /// </summary>
        /// <param name="path">The string value that represents the path to be used to find the tagged
        /// path that represents it</param>
        /// <param name="filtered">The boolean value that represents whether to filter out tagged paths
        /// which are set as deleted</param>
        /// <returns>the tagged path that represents the path that is passed as parameter; otherwise null
        /// </returns>
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
        
        /// <summary>
        /// Finds the tagged path that represents the path that is passed as parameter from the filtered 
        /// list of tagged paths
        /// </summary>
        /// <param name="path">The string value that represents the path to be used to find the tagged
        /// path that represents it</param>
        /// <returns>the tagged path that represents the path that is passed as parameter; otherwise null
        /// </returns>
        public TaggedPath FindPath(string path)
        {
            return FindPath(path, true);
        }

        /// <summary>
        /// Finds the list of paths which are the ancestor path of the path that is passed as parameter
        /// </summary>
        /// <param name="path">The string value that represents the path that is to be used to find
        /// its ancestor paths</param>
        /// <returns>the list of paths which are the ancestor paths of the path that is passed as parameter
        /// </returns>
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
        /// Finds the list of paths which are the descendant path of the path that is passed as parameter
        /// </summary>
        /// <param name="path">The string value that represents the path that is to be used to find
        /// its descendant paths</param>
        /// <returns>the list of paths which are the descendant paths of the path that is passed as parameter
        /// </returns>
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
    }
}
