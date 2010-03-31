using System.Collections.Generic;
using Syncless.Tagging.Exceptions;
using Syncless.Filters;
using Syncless.Helper;

namespace Syncless.Tagging
{
    public class TaggingProfile
    {
        private const int TAG_ALREADY_EXISTS = 1;
        private const int TAG_NOT_FOUND = 2;
        
        private string _profileName;
        private long _lastUpdatedDate;
        private long _createdDate;
        private List<Tag> _tagList;

        public string ProfileName
        {
            get { return _profileName; }
            set { _profileName = value; }
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
        public List<Tag> TagList
        {
            get { return _tagList; }
            set { _tagList = value; }
        }
        public List<string> AllFilteredTaggedPathList
        {
            get
            {
                List<string> pathList = new List<string>();
                foreach (Tag tag in _tagList)
                {
                    foreach (TaggedPath path in tag.FilteredPathList)
                    {
                        if (!PathHelper.ContainsIgnoreCase(pathList, path.PathName))
                        {
                            pathList.Add(path.PathName);
                        }
                    }
                }
                return pathList;
            }
        }
        public List<Tag> ReadOnlyTagList
        {
            get
            {
                lock (_tagList)
                {
                    List<Tag> readOnlyTagList = new List<Tag>();
                    foreach (Tag t in _tagList)
                    {
                        readOnlyTagList.Add(t);
                    }
                    return readOnlyTagList;
                }
            }
        }

        public TaggingProfile(long created)
        {
            _tagList = new List<Tag>();
            _createdDate = created;
            _lastUpdatedDate = created;
            _profileName = "";
        }

        public Tag AddTag(string tagname)
        {
            Tag t = FindTag(tagname);
            if (t != null)
            {
                if (t.IsDeleted)
                {
                    CurrentTime current = new CurrentTime();
                    lock (_tagList)
                    {
                        _tagList.Remove(t);
                    }
                    Tag tag = new Tag(tagname, current.CurrentTimeLong);
                    lock (_tagList)
                    {
                        _tagList.Add(tag);
                    }
                    _lastUpdatedDate = current.CurrentTimeLong;
                    return tag;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                CurrentTime current = new CurrentTime();
                Tag tag = new Tag(tagname, current.CurrentTimeLong);
                lock (_tagList)
                {
                    _tagList.Add(tag);
                }
                _lastUpdatedDate = current.CurrentTimeLong; ;
                return tag;
            }
        }

        public Tag AddTag(Tag tag)
        {
            Tag t = FindTag(tag.TagName);
            if (t != null)
            {
                if (t.IsDeleted)
                {
                    lock (_tagList)
                    {
                        _tagList.Remove(t);
                        _tagList.Add(tag);
                    }
                    _lastUpdatedDate = TaggingHelper.GetCurrentTime();
                    return tag;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                lock (_tagList)
                {
                    _tagList.Add(tag);
                }
                _lastUpdatedDate = TaggingHelper.GetCurrentTime();
                return tag;
            }
        }

        public int RenameTag(string oldname, string newname)
        {
            Tag torename = FindTag(oldname);
            if (torename != null)
            {
                if (!Contains(newname))
                {
                    torename.TagName = newname;
                    _lastUpdatedDate = TaggingHelper.GetCurrentTime();
                    return 0;
                }
                else
                {
                    return TAG_ALREADY_EXISTS;
                }
            }
            else
            {
                return TAG_NOT_FOUND;
            }
        }

        public Tag DeleteTag(string tagname)
        {
            Tag toRemove = FindTag(tagname);
            if (toRemove != null)
            {
                if (toRemove.IsDeleted)
                {
                    return null;
                }
                else
                {
                    CurrentTime updated = new CurrentTime();
                    toRemove.Remove(updated.CurrentTimeLong);
                    _lastUpdatedDate = updated.CurrentTimeLong;
                    return toRemove;
                }
            }
            else
            {
                return null;
            }
        }

        public Tag DeleteTag(Tag tag)
        {
            Tag toRemove = FindTag(tag.TagName);
            if (toRemove != null)
            {
                if (toRemove.IsDeleted)
                {
                    return null;
                }
                else
                {
                    CurrentTime updated = new CurrentTime();
                    toRemove.Remove(updated.CurrentTimeLong);
                    _lastUpdatedDate = updated.CurrentTimeLong;
                    return toRemove;
                }
            }
            else
            {
                return null;
            }
        }

        public Tag TagFolder(string path, string tagname)
        {
            CurrentTime current = new CurrentTime();
            Tag toTag = FindTag(tagname);
            if (toTag == null)
            {
                Tag tag = new Tag(tagname, current.CurrentTimeLong);
                tag.AddPath(path, current.CurrentTimeLong);
                lock (_tagList)
                {
                    _tagList.Add(tag);
                }
                _lastUpdatedDate = current.CurrentTimeLong;
                return tag;
            }
            else
            {
                if (toTag.IsDeleted)
                {
                    lock (_tagList)
                    {
                        _tagList.Remove(toTag);
                    }
                    Tag tag = new Tag(tagname, current.CurrentTimeLong);
                    tag.AddPath(path, current.CurrentTimeLong);
                    lock (_tagList)
                    {
                        _tagList.Add(tag);
                    }
                    _lastUpdatedDate = current.CurrentTimeLong;
                    return tag;
                }
                else
                {
                    if (toTag.Contains(path))
                    {
                        throw new PathAlreadyExistsException(path);
                    }
                    else if (TaggingHelper.CheckRecursiveDirectory(toTag, path))
                    {
                        throw new RecursiveDirectoryException(path, tagname);
                    }
                    else
                    {
                        toTag.AddPath(path, current.CurrentTimeLong);
                        _lastUpdatedDate = current.CurrentTimeLong;
                        return toTag;
                    }
                }
            }
        }

        public int UntagFolder(string path, string tagname)
        {
            Tag tag = FindTag(tagname);
            if (tag != null)
            {
                CurrentTime current = new CurrentTime();
                if (tag.RemovePath(path, current.CurrentTimeLong))
                {
                    _lastUpdatedDate = current.CurrentTimeLong;
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return -1;
            }
        }

        public int UntagFolder(string path)
        {
            int noOfPath = 0;
            CurrentTime current = new CurrentTime();
            foreach (Tag tag in _tagList)
            {
                if (tag.Contains(path))
                {
                    if (tag.RemovePath(path, current.CurrentTimeLong))
                    {
                        _lastUpdatedDate = current.CurrentTimeLong;
                        noOfPath++;
                    }
                }
            }
            return noOfPath;
        }

        public int RenameFolder(string oldpath, string newpath)
        {
            int renamedCount = 0;
            foreach (Tag tag in _tagList)
            {
                if (tag.ContainsParent(oldpath))
                {
                    if (!tag.ContainsParent(newpath))
                    {
                        CurrentTime current = new CurrentTime();
                        renamedCount += tag.RenamePath(oldpath, newpath, current.CurrentTimeLong);
                        _lastUpdatedDate = current.CurrentTimeLong;
                    }
                }
            }
            return renamedCount;
        }

        public bool UpdateFilter(string tagname, List<Filter> newFilterList)
        {
            Tag tag = FindTag(tagname);
            if (tag != null)
            {
                tag.UpdateFilter(newFilterList);
                _lastUpdatedDate = TaggingHelper.GetCurrentTime();
                return true;
            }
            else
            {
                return false;
            }
        }

        public Tag FindTag(string tagname)
        {
            foreach (Tag tag in _tagList)
            {
                if (tag.TagName.ToLower().Equals(tagname.ToLower()))
                {
                    return tag;
                }
            }
            return null;
        }

        public List<Tag> RetrieveAllTags(bool getdeleted)
        {
            if (getdeleted)
            {
                return _tagList;
            }
            else
            {
                List<Tag> pathList = new List<Tag>();
                foreach (Tag tag in _tagList)
                {
                    if (!tag.IsDeleted)
                    {
                        pathList.Add(tag);
                    }
                }
                return pathList;
            }
        }

        public List<Tag> RetrieveTagsByPath(string path)
        {
            List<Tag> tagList = new List<Tag>();
            foreach (Tag tag in _tagList)
            {
                if (tag.Contains(path))
                {
                    tagList.Add(tag);
                }
            }
            return tagList;
        }

        /// <summary>
        /// Check if this tagging profile contains the given tag, only if it is not set as deleted
        /// </summary>
        /// <param name="path">The name of the tag to find</param>
        /// <returns>True if tagging profile contains the given tag</returns>
        public bool Contains(string tagname)
        {
            foreach (Tag tag in _tagList)
            {
                if (tag.TagName.ToLower().Equals(tagname.ToLower()))
                {
                    if (tag.IsDeleted)
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

        /// <summary>
        /// Check if this tagging profile contains the given tag regardless of whether it is set as 
        /// deleted or not
        /// </summary>
        /// <param name="path">The name of the tag to find</param>
        /// <returns>True if tagging profile contains the given tag</returns>
        public bool ContainsIgnoreDeleted(string tagname)
        {
            foreach (Tag tag in _tagList)
            {
                if (tag.TagName.ToLower().Equals(tagname.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
