using System.Collections.Generic;
using System.IO;
using Syncless.Helper;
using Syncless.Tagging.Exceptions;
using System.Diagnostics;
using Syncless.Filters;
using Syncless.Logging;

namespace Syncless.Tagging
{
    public class TaggingLayer
    {
        public const string RELATIVE_TAGGING_SAVE_PATH = ".syncless\\tagging.xml";
        public const string RELATIVE_TAGGING_ROOT_SAVE_PATH = "tagging.xml";

        #region attributes
        private static TaggingLayer _instance;
        private TaggingProfile _taggingProfile;

        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static TaggingLayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TaggingLayer();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Contains information about the current profile name and the tag lists
        /// </summary>
        public TaggingProfile TaggingProfile
        {
            get { return _taggingProfile; }
            set { _taggingProfile = value; }
        }
        
        /// <summary>
        /// Contains an immutable list of Tag objects
        /// </summary>
        public List<Tag> TagList
        {
            get { return _taggingProfile.TagList; }
        }

        /// <summary>
        /// Contains a copy of the list of all Tag objects
        /// </summary>
        public List<Tag> UnfilteredTagList
        {
            get
            {
                List<Tag> allTagList = new List<Tag>();
                foreach (Tag tag in _taggingProfile.TagList)
                {
                    allTagList.Add(tag);
                }
                return allTagList;
            }
        }
        #endregion

        /// <summary>
        /// Contains a copy of the list of Tag objects which are not set as deleted
        /// </summary>
        public List<Tag> FilteredTagList
        {
            get 
            { 
                List<Tag> filteredTagList = new List<Tag>();
                foreach (Tag tag in _taggingProfile.TagList)
                {
                    if (!tag.IsDeleted)
                    {
                        filteredTagList.Add(tag);
                    }
                }
                return filteredTagList;
            }
        }

        private TaggingLayer()
        {
            _taggingProfile = new TaggingProfile(TaggingHelper.GetCurrentTime());
        }

        /// <summary>
        /// Initialize _taggingProfile object. If a tagging.xml file has already been created, load the information
        /// from the file, else, instantiate a new _taggingProfile object.
        /// </summary>
        /// <param name="paths">The paths of the tagging.xml files to be loaded.</param>
        public void Init(List<string> paths)
        {
            string profileFilePath = paths[0]; //paths[0] is always the root.
            if (!File.Exists(profileFilePath))
            {
                _taggingProfile = new TaggingProfile(TaggingHelper.GetCurrentTime());
            }
            else
            {
                _taggingProfile = TaggingXMLHelper.LoadFrom(profileFilePath);
                Debug.Assert(_taggingProfile != null);
            }
            for (int i = 1; i < paths.Count; i++)
            {
                if (File.Exists(paths[i]))
                {
                    TaggingProfile profile = TaggingXMLHelper.LoadFrom(paths[i]);
                    int updateCount = TagMerger.MergeProfile(_taggingProfile, profile);
                }
            }
        }
        
        /// <summary>
        /// Merge the profile given in the path to the current
        /// </summary>
        /// <param name="path">The path of the xml file</param>
        /// <returns>True if merge is successful, else false</returns>
        public bool Merge(string path)
        {
            if (File.Exists(path))
            {
                TaggingProfile profile = TaggingXMLHelper.LoadFrom(path);
                int updateCount = TagMerger.MergeProfile(_taggingProfile, profile);
                return true;
            }
            else
            {
                return false;
            }
        }

        #region Tag public implementations
        /// <summary>
        /// Create a Tag of tagname
        /// </summary>
        /// <param name="tagname">The name of the Tag to be created</param>
        /// <returns>The created Tag, else raise TagAlreadyExistsException</returns>
        public Tag CreateTag(string tagname)
        {
            Tag tag = _taggingProfile.AddTag(tagname);
            if (tag != null)
            {
                TaggingHelper.Logging(LogEventType.APPEVENT_TAG_CREATED, LogMessage.TAG_CREATED, tagname);
                return tag;
            }
            else
            {
                throw new TagAlreadyExistsException(tagname);
            }
        }

        /// <summary>
        /// Rename a Tag of oldname to newname
        /// </summary>
        /// <param name="oldname">The original name of the Tag to be renamed</param>
        /// <param name="newname">The new name to be given to the Tag</param>
        /// <returns>If the oldname does not exist, raise TagNotFoundException, if newname is already used
        /// for another Tag, raise TagAlreadyExistsException</returns>
        public void RenameTag(string oldname, string newname)
        {
            int result = _taggingProfile.RenameTag(oldname, newname);
            switch (result)
            {
                case 0:
                    //TaggingHelper.Logging(LogMessage.TAG_RENAMED, oldname, newname);
                    break;
                case 1:
                    throw new TagAlreadyExistsException(newname);
                case 2:
                    throw new TagNotFoundException(oldname);
                default:
                    //assertion
                    break;
            }
        }

        /// <summary>
        /// Remove the Tag of tagname
        /// </summary>
        /// <param name="tagname">The name of the Tag to be removed</param>
        /// <returns>The Tag that is removed successfully, else raise TagNotFoundException</returns>
        public Tag DeleteTag(string tagname)
        {
            Tag toremove = _taggingProfile.DeleteTag(tagname);
            if (toremove != null)
            {
                TaggingHelper.Logging(LogEventType.APPEVENT_TAG_DELETED, LogMessage.TAG_REMOVED, tagname);
                return toremove;
            }
            else
            {
                throw new TagNotFoundException(tagname);
            }
        }

        /// <summary>
        /// Tag a folder with a tagname
        /// </summary>
        /// <param name="path">The path of the folder to be tagged.</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>The Tag that contains the path, if path already exists raise PathAlreadyExistsException
        /// if the given path has sub-directory or parent directory already tagged raise 
        /// RecursiveDirectoryException</returns>
        public Tag TagFolder(string path, string tagname)
        {
            try
            {
                Tag tag = _taggingProfile.TagFolder(path, tagname);
                TaggingHelper.Logging(LogEventType.APPEVENT_FOLDER_TAGGED, LogMessage.FOLDER_TAGGED, path, tagname);
                return tag;
            }
            catch (PathAlreadyExistsException paee)
            {
                throw paee;
            }
            catch (RecursiveDirectoryException rde)
            {
                throw rde;
            }
        }

        /// <summary>
        /// Untag a Folder from a tagname
        /// </summary>
        /// <param name="path">The path to untag</param>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>1 if the path is removed, 0 if the path is not found in the Tag, else raise 
        /// TagNotFoundException</returns>
        public int UntagFolder(string path, string tagname)
        {
            int result = _taggingProfile.UntagFolder(path, tagname);
            switch (result)
            {
                case 0:
                    return 0;
                case 1:
                    TaggingHelper.Logging(LogEventType.APPEVENT_FOLDER_UNTAGGED, LogMessage.FOLDER_UNTAGGED, path, tagname);
                    return 1;
                default: 
                    throw new TagNotFoundException(tagname);
            }
        }

        /// <summary>
        /// Untag the path in all the Tags it is tagged to
        /// </summary>
        /// <param name="path">The name of the path to be untagged</param>
        /// <returns>The number of Tags the path is untagged from</returns>
        public int UntagFolder(string path)
        {
            return _taggingProfile.UntagFolder(path);
        }

        /// <summary>
        /// Update the list of Filters for a Tag of tagname
        /// </summary>
        /// <param name="tagname">The name of the Tag</param>
        /// <param name="newFilterList">The list of new Filters</param>
        public void UpdateFilter(string tagname, List<Filter> newFilterList)
        {
            if (!_taggingProfile.UpdateFilter(tagname, newFilterList))
            {
                throw new TagNotFoundException(tagname);
            }
        }

        /// <summary>
        /// Rename a path in all the Tags it is tagged to
        /// </summary>
        /// <param name="oldPath">The original path of the folder</param>
        /// <param name="newPath">The new path of the folder</param>
        public int RenameFolder(string oldPath, string newPath)
        {
            return _taggingProfile.RenameFolder(oldPath, newPath);
        }

        /// <summary>
        /// Retrieve the Tag with the particular tag name
        /// </summary>
        /// <param name="tagname">The name of the Tag</param>
        /// <returns>The Tag that is to be found, else null</returns>
        public Tag RetrieveTag(string tagname)
        {
            return RetrieveTag(tagname, false, 0);
        }

        /// <summary>
        /// Retrieve the Tag with the particular tag name
        /// </summary>
        /// <param name="tagname">The name of the Tag</param>
        /// <param name="getdeleted">Indicate whether to return the Tag if its IsDeleted property is true</param>
        /// <returns>The Tag that is to be found, else null</returns>
        public Tag RetrieveTag(string tagname, bool getdeleted)
        {
            return RetrieveTag(tagname, false, getdeleted, 0);
        }

        /// <summary>
        /// Retrieve all Tags
        /// </summary>
        /// <param name="getdeleted">Indicate whether to return a Tag which has been deleted</param>
        /// <returns>A list of Tags</returns>
        public List<Tag> RetrieveAllTags(bool getdeleted)
        {
            return _taggingProfile.RetrieveAllTags(getdeleted);
        }

        /// <summary>
        /// Retrieve a list of Tags where a given path is tagged to
        /// </summary>
        /// <param name="path">The path to find the Tags it is tagged to</param>
        /// <returns>The list of Tags containing the given path</returns>
        public List<Tag> RetrieveTagByPath(string path)
        {
            return _taggingProfile.RetrieveTagsByPath(path);
        }
        
        /// <summary>
        /// Retrieve all paths having logicalid
        /// </summary>
        /// <param name="logicalid">The logical ID</param>
        /// <returns>The list of paths having logicalid</returns>
        public List<string> RetrievePathByLogicalId(string logicalid)
        {
            List<string> pathList = new List<string>();
            List<Tag> tagList = RetrieveTagByLogicalId(logicalid);
            foreach (Tag tag in tagList)
            {
                foreach (TaggedPath path in tag.FilteredPathList)
                {
                    //try && conditions
                    if (path.LogicalDriveId.Equals(logicalid))
                    {
                        if (!PathHelper.ContainsIgnoreCase(pathList, path.PathName))
                        {
                            pathList.Add(path.PathName);
                        }
                    }
                }
            }
            return pathList;
        }

        /// <summary>
        /// Retrieve all the tags that have path in a logical drive having logicalid.
        /// </summary>
        /// <param name="logicalId">The Logical Id</param>
        /// <returns>The list of Tags</returns>
        public List<Tag> RetrieveTagByLogicalId(string logicalid)
        {
            bool found;
            List<Tag> tagList = new List<Tag>();
            foreach (Tag tag in _taggingProfile.TagList)
            {
                found = CheckID(tag, logicalid);
                if (found)
                {
                    tagList.Add(tag);
                }
            }
            return tagList;
        }

        /// <summary>
        /// Find a list of paths of folders or sub-folders which share the same Tag as folderPath
        /// Example: TagA - D:\A\, E:\B\C\
        ///          TagB - D:\A\, F:\D\E\G\
        ///          TagC - E:\G\, F:\H\
        ///          Given path D:\A\H\J\
        ///          Should return E:\B\C\H\J\ from TagA, F:\D\E\G\H\J\ from TagB
        /// </summary>
        /// <param name="folderPath">The path to search</param>
        /// <returns>The list of similar paths</returns>
        public List<string> FindSimilarPathForFolder(string folderPath)
        {
            string logicalid = TaggingHelper.GetLogicalID(folderPath);
            List<string> pathList = new List<string>();
            foreach (Tag tag in _taggingProfile.TagList)
            {
                if (tag.Contains(folderPath))
                {
                    foreach (TaggedPath p in tag.FilteredPathList)
                    {
                        if (!PathHelper.ContainsIgnoreCase(pathList, p.PathName) && !PathHelper.EqualsIgnoreCase(p.PathName, folderPath))
                        {
                            pathList.Add(p.PathName);
                        }
                    }
                }
            }
            List<Tag> matchingTag = RetrieveTagByLogicalId(logicalid);
            foreach (Tag tag in matchingTag)
            {
                string appendedPath;
                string trailingPath = tag.CreateTrailingPath(folderPath, true);
                if (trailingPath != null)
                {
                    foreach (TaggedPath p in tag.FilteredPathList)
                    {
                        appendedPath = p.Append(trailingPath);
                        if (!PathHelper.ContainsIgnoreCase(pathList, appendedPath) && !PathHelper.EqualsIgnoreCase(appendedPath, folderPath))
                        {
                            pathList.Add(appendedPath);
                        }
                    }
                }
            }
            return pathList;
        }

        /// <summary>
        /// Retrieve all the tags that contain the parent folder paths of the given path
        /// </summary>
        /// <param name="path">The path used to retrieve the parent folder paths</param>
        /// <returns>The list of Tags containing the parent folder paths</returns>
        public List<Tag> RetrieveParentTagByPath(string path)
        {
            List<Tag> parentPathList = new List<Tag>();

            foreach (Tag tag in _taggingProfile.TagList)
            {
                foreach (TaggedPath p in tag.FilteredPathList)
                {
                    if (PathHelper.StartsWithIgnoreCase(path, p.PathName))
                    {
                        if (!PathHelper.EqualsIgnoreCase(path, p.PathName))
                        {
                            parentPathList.Add(tag);
                            break;
                        }
                    }
                }
            }
            return parentPathList;
        }

        /// <summary>
        /// Retrieve a list of tagged parent folder paths of the given path
        /// </summary>
        /// <param name="path">The path used to retrieve the parent folder paths</param>
        /// <returns>The list of folder paths</returns>
        public List<string> RetrieveAncestors(string path)
        {
            List<string> ancestors = new List<string>();
            foreach (Tag tag in _taggingProfile.TagList)
            {
                foreach (string found in tag.FindAncestors(path))
                {
                    if (!PathHelper.ContainsIgnoreCase(ancestors, found))
                    {
                        ancestors.Add(found);
                    }
                }
            }
            return ancestors;
        }

        /// <summary>
        /// Retrieve a list of tagged child folder paths of the given path
        /// </summary>
        /// <param name="path">The path used to retrieve the child folder paths</param>
        /// <returns>The list of child folder paths</returns>
        public List<string> RetrieveDescendants(string path)
        {
            List<string> descendants = new List<string>();
            foreach (Tag tag in FilteredTagList)
            {
                foreach (string found in tag.FindDescendants(path))
                {
                    if (!PathHelper.ContainsIgnoreCase(descendants, found))
                    {
                        descendants.Add(found);
                    }
                }
            }
            return descendants;
        }

        /// <summary>
        /// Get all the tagged paths
        /// </summary>
        /// <returns>The list of tagged paths</returns>
        public List<string> GetAllPaths()
        {
            return _taggingProfile.AllFilteredTaggedPathList;
        }

        /// <summary>
        /// Check if a logicalid exists
        /// </summary>
        /// <param name="logicalid">The logicalid to be checked</param>
        /// <returns>True if the logicalid is found, else false</returns>
        public bool CheckIDExists(string logicalid)
        {
            foreach (Tag tag in _taggingProfile.TagList)
            {
                if (CheckID(tag, logicalid))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Save information of a profile to a xml file
        /// </summary>
        /// <param name="xmlFilePath">The path of the tagging.xml file to be saved to</param>
        /// <returns></returns>
        public void SaveTo(List<string> savedLocation)
        {
            TaggingXMLHelper.SaveToLocations(_taggingProfile, savedLocation);
        }

        /// <summary>
        /// Append the current profile to tagging.xml saved in the given list of locations
        /// </summary>
        /// <param name="savedLocation">The list of locations containing tagging.xml where current profile
        /// is to be saved to</param>
        public void AppendProfile(List<string> savedLocation)
        {
            TaggingXMLHelper.AppendProfile(_taggingProfile, savedLocation);
        }

        #region for Merger
        /// <summary>
        /// Add a Tag to the current tagging profile. Used for merging Tag objects from several tagging profiles.
        /// </summary>
        /// <param name="tag">The Tag to be added.</param>
        public void AddTag(Tag tag)
        {
            _taggingProfile.AddTag(tag);
        }
        #endregion
        #endregion

        #region private methods implementations
        #region completed
        private Tag RetrieveTag(string tagname, bool create, bool getdeleted, long lastupdated)
        {
            Tag tag = GetTag(tagname);
            if (tag != null)
            {
                if (tag.IsDeleted)
                {
                    if (getdeleted)
                    {
                        return tag;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return tag;
                }
            }
            else
            {
                if (create)
                {
                    tag = new Tag(tagname, lastupdated);
                }
                return tag;
            }
        }

        private Tag RetrieveTag(string tagname, bool create, long lastupdated)
        {
            Tag tag = GetTag(tagname);
            if (tag == null)
            {
                if (create)
                {
                    tag = new Tag(tagname, lastupdated);
                }
            }
            return tag;
        }

        private Tag GetTag(string tagname)
        {
            foreach (Tag tag in _taggingProfile.TagList)
            {
                if (TaggingHelper.FormatTagName(tag.TagName).Equals(TaggingHelper.FormatTagName(tagname)))
                {
                    return tag;
                }
            }
            return null;
        }

        private bool CheckTagExists(string tagname)
        {
            foreach (Tag tag in _taggingProfile.TagList)
            {
                if (TaggingHelper.FormatTagName(tag.TagName).Equals(TaggingHelper.FormatTagName(tagname)))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckID(Tag tag, string ID)
        {
            foreach (TaggedPath path in tag.FilteredPathList)
            {
                if (path.LogicalDriveId.Equals(ID))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
        #endregion
    }
}
