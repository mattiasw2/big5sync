/*
 * 
 * Author: Goh Khoon Hiang
 * 
 */

using System.Collections.Generic;
using Syncless.Tagging.Exceptions;
using Syncless.Filters;
using Syncless.Helper;

namespace Syncless.Tagging
{
    /// <summary>
    /// TaggingProfile class represents a container for a list of <see cref="Tag">Tag</see> objects.
    /// Each TaggingProfile has its properties that uniquely identifies itself from other TaggingProfile 
    /// objects.
    /// </summary>
    public class TaggingProfile
    {
        /// <summary>
        /// The integer value that indicates a tag already exists
        /// </summary>
        private const int TAG_ALREADY_EXISTS = 1;

        /// <summary>
        /// The integer value that indicates a tag is not found
        /// </summary>
        private const int TAG_NOT_FOUND = 2;
        
        private string _profileName;
        private long _lastUpdatedDate;
        private long _createdDate;
        private List<Tag> _tagList;

        /// <summary>
        /// Gets or sets the name of the tagging profile
        /// </summary>
        public string ProfileName
        {
            get { return _profileName; }
            set { _profileName = value; }
        }

        /// <summary>
        /// Gets or sets the last updated date of the tagging profile
        /// </summary>
        public long LastUpdatedDate
        {
            get { return _lastUpdatedDate; }
            set { _lastUpdatedDate = value; }
        }

        /// <summary>
        /// Gets or sets the created date of the tagging profile
        /// </summary>
        public long CreatedDate
        {
            get { return _createdDate; }
            set { _createdDate = value; }
        }

        /// <summary>
        /// Gets or sets the list of tags
        /// </summary>
        public List<Tag> TagList
        {
            get { return _tagList; }
            set { _tagList = value; }
        }

        /// <summary>
        /// Gets a clone of the list of full path name of tagged paths which are not set as deleted 
        /// </summary>
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

        /// <summary>
        /// Gets a clone of the list of tags
        /// </summary>
        public List<Tag> ReadOnlyTagList
        {
            get
            {
                lock (TagList)
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

        /// <summary>
        /// Creates a new TaggingProfile object
        /// </summary>
        /// <param name="created">The long value that represents the created date of the tagging profile</param>
        /// <remarks>The last updated date is set to the created date. The profile name is set to empty
        /// by default.</remarks>
        public TaggingProfile(long created)
        {
            _tagList = new List<Tag>();
            _createdDate = created;
            _lastUpdatedDate = created;
            _profileName = "";
        }

        /// <summary>
        /// Adds a tag with the tag name that is passed as parameter to the list of tags
        /// </summary>
        /// <param name="tagname">The string value that represents the name of the tag to be added</param>
        /// <returns>the tag if it is added; otherwise, null</returns>
        public Tag AddTag(string tagname)
        {
            Tag t = FindTag(tagname);
            if (t != null)
            {
                if (t.IsDeleted)
                {
                    CurrentTime current = new CurrentTime();
                    lock (TagList)
                    {
                        _tagList.Remove(t);
                    }
                    Tag tag = new Tag(tagname, current.CurrentTimeLong);
                    lock (TagList)
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
                lock (TagList)
                {
                    _tagList.Add(tag);
                }
                _lastUpdatedDate = current.CurrentTimeLong; ;
                return tag;
            }
        }

        /// <summary>
        /// Adds a tag that is passed as parameter to the list of tags
        /// </summary>
        /// <param name="tag">The <see cref="Tag">Tag</see> object that represents the tag to be added</param>
        /// <returns>the tag if it is added; otherwise, null</returns>
        public Tag AddTag(Tag tag)
        {
            Tag t = FindTag(tag.TagName);
            if (t != null)
            {
                if (t.IsDeleted)
                {
                    lock (TagList)
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
                lock (TagList)
                {
                    _tagList.Add(tag);
                }
                _lastUpdatedDate = TaggingHelper.GetCurrentTime();
                return tag;
            }
        }

        /// <summary>
        /// Sets the name of a tag that is the same as the old name to the new name that is passed as parameter
        /// </summary>
        /// <param name="oldname">The string value that represents the old name of a tag</param>
        /// <param name="newname">The string value that represents the new name to be given to the tag</param>
        /// <returns>0 if the old name of the tag is set to the new name that is passed as parameter,
        /// 1 if the new name is already used by an existing tag, 2 if the old name is not found in
        /// any existing tag</returns>
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

        /// <summary>
        /// Removes the tag whose tag name is the same as the tag name that is passed as parameter
        /// </summary>
        /// <param name="tagname">The string value that represents the name that is to be used to retrieve
        /// the tag to be deleted</param>
        /// <returns>the tag that is deleted if it is deleted; otherwise, null</returns>
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

        /// <summary>
        /// Removes the tag that is passed as parameter from the list of tags
        /// </summary>
        /// <param name="tag">The <see cref="Tag">Tag</see> object that represents the tag to be removed</param>
        /// <returns>the tag that is deleted if it is deleted; otherwise, null</returns>
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

        /// <summary>
        /// Tags a folder path to a tag with name same as the tag name that is passed as parameter
        /// </summary>
        /// <param name="path">The string value that represents the path of the folder to be tagged</param>
        /// <param name="tagname">The string value that represents the name of the tag the folder path is
        /// to be tagged to</param>
        /// <returns>the tag where the folder path is tagged to</returns>
        /// <exception cref="PathAlreadyExistsException">thrown if the folder path that is passed as 
        /// parameter is already tagged to the tag</exception>
        /// <exception cref="RecursiveDirectoryException">thrown if the folder path that is passed as 
        /// parameter is a parent path or a child path of another path that is already tagged to 
        /// the tag</exception>
        public Tag TagFolder(string path, string tagname)
        {
            CurrentTime current = new CurrentTime();
            Tag toTag = FindTag(tagname);
            if (toTag == null)
            {
                Tag tag = new Tag(tagname, current.CurrentTimeLong);
                tag.AddPath(path, current.CurrentTimeLong);
                lock (TagList)
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
                    lock (TagList)
                    {
                        _tagList.Remove(toTag);
                    }
                    Tag tag = new Tag(tagname, current.CurrentTimeLong);
                    tag.AddPath(path, current.CurrentTimeLong);
                    lock (TagList)
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

        /// <summary>
        /// Untags a folder path from a tag with name same as the tag name that is passed as parameter
        /// </summary>
        /// <param name="path">The string value that represents the path of the folder to be untagged</param>
        /// <param name="tagname">The string value that represents the name of the tag the folder path is
        /// to be untagged from</param>
        /// <returns>1 if the path is removed, 0 if the path is not found in the Tag, -1 if the name that 
        /// is passed as parameter is not used by any tag in the existing list of tags</returns>
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

        /// <summary>
        /// Untags a folder path from all tags where the folder path is tagged to
        /// </summary>
        /// <param name="path">The string value that represents the path of the folder to be untagged</param>
        /// <returns>the number of tags the path is untagged from</returns>
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

        /// <summary>
        /// Sets the name of a folder path name that is the same as the old path name to the new path 
        /// name that is passed as parameter
        /// </summary>
        /// <param name="oldpath">The string value that represents the old name of a folder path</param>
        /// <param name="newpath">The string value that represents the new name of a folder path</param>
        /// <returns>the number of folder paths whose old name is replaced by the new name</returns>
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

        /// <summary>
        /// Updates the list of filters for a tag with name same as the tag name that is passed as parameter
        /// </summary>
        /// <param name="tagname">The string value that represents the name that is to be used to retrieve
        /// the tag</param>
        /// <param name="newFilterList">The list of filters that is to be updated to the tag</param>
        /// <returns>true if the tag exists; otherwise, false</returns>
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

        /// <summary>
        /// Finds the tag that has name same with the tag name passed as parameter
        /// </summary>
        /// <param name="tagname">The string value that represents the tag name of the tag to be 
        /// retrieved</param>
        /// <returns>the tag if it is found; otherwise, null</returns>
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

        /// <summary>
        /// Gets the list of all tags
        /// </summary>
        /// <param name="getdeleted">The boolean value that represents whether to return the tag if it set
        /// as deleted</param>
        /// <returns>the list of all tags</returns>
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

        /// <summary>
        /// Gets a list of tags where a path, that is passed as parameter, is tagged to
        /// </summary>
        /// <param name="path">The string value that represents the path of the folder to be used to
        /// retrieve a list of tags</param>
        /// <returns>the list of tags where the path is tagged to</returns>
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
        /// Checks whether a tag that is not set as deleted with name same as the tag name 
        /// passed as parameter exists
        /// </summary>
        /// <param name="tagname">The string value that represents the tag name of the tag to be checked</param>
        /// <returns>true if the tag is found; otherwise, false</returns>
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
        /// Checks whether a tag with name same as the tag name passed as parameter exists
        /// </summary>
        /// <param name="tagname">The string value that represents the tag name of the tag to be checked</param>
        /// <returns>true if the tag is found; otherwise, false</returns>
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
