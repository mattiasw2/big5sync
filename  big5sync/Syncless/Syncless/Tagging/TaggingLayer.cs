using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.Helper;
using Syncless.Tagging.Exceptions;
using System.Diagnostics;
using Syncless.Filters;

namespace Syncless.Tagging
{
    public class TaggingLayer
    {
        public const string RELATIVE_PROFILING_SAVE_PATH = "\\.syncless\\tagging.xml";
        public const string RELATIVE_TAGGING_ROOT_SAVE_PATH = "tagging.xml";
        #region attributes
        private static TaggingLayer _instance;

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
        /// Contains a list of Tag objects
        /// </summary>
        public List<Tag> TagList
        {
            get { return _taggingProfile.TagList; }
        }

        /// <summary>
        /// Contains a list of all Tag objects
        /// </summary>
        public List<Tag> AllTagList
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

        private TaggingProfile _taggingProfile;

        /// <summary>
        /// Contains information about the current profile name and the tag lists
        /// </summary>
        public TaggingProfile TaggingProfile
        {
            get { return _taggingProfile; }
            set { _taggingProfile = value; }
        }
        #endregion

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
                if(File.Exists(paths[i])){
                    TaggingProfile profile = TaggingXMLHelper.LoadFrom(paths[i]);
                    int updateCount = TagMerger.MergeProfile(_taggingProfile, profile);
                }
            }
        }

        #region Tag public implementations
        /// <summary>
        /// Create a Tag of tagname
        /// </summary>
        /// <param name="tagname">The name of the Tag to be created</param>
        /// <returns>The created Tag, else raise TagAlreadyExistsException</returns>
        #region deprecated
        //public Tag CreateTag(string tagname)
        //{
        //    CurrentTime created = new CurrentTime();
        //    if (!CheckTagExists(tagname))
        //    {
        //        Tag tag = new Tag(tagname, created.CurrentTimeLong);
        //        _taggingProfile.TagList.Add(tag);
        //        UpdateTaggingProfileDate(created.CurrentTimeLong);
        //        TaggingHelper.Logging(LogMessage.TAG_CREATED, tagname);
        //        return tag;
        //    }
        //    else
        //    {
        //        TaggingHelper.Logging(LogMessage.TAG_ALREADY_EXISTS, tagname);
        //        throw new TagAlreadyExistsException(tagname);
        //    }
        //}
        #endregion
        public Tag CreateTag(string tagname)
        {
            CurrentTime created = new CurrentTime();
            Tag toremove = GetTag(tagname);
            if (toremove != null)
            {
                if (toremove.IsDeleted)
                {
                    _taggingProfile.TagList.Remove(toremove);
                    Tag toadd = new Tag(tagname, created.CurrentTimeLong);
                    _taggingProfile.TagList.Add(toadd);
                    UpdateTaggingProfileDate(created.CurrentTimeLong);
                    TaggingHelper.Logging(LogMessage.TAG_CREATED, tagname);
                    return toadd;
                }
                else
                {
                    TaggingHelper.Logging(LogMessage.TAG_ALREADY_EXISTS, tagname);
                    throw new TagAlreadyExistsException(tagname);
                }
            }
            else
            {
                Tag toadd = new Tag(tagname, created.CurrentTimeLong);
                _taggingProfile.TagList.Add(toadd);
                UpdateTaggingProfileDate(created.CurrentTimeLong);
                TaggingHelper.Logging(LogMessage.TAG_CREATED, tagname);
                return toadd;
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
            CurrentTime updated = new CurrentTime();
            if (CheckTagExists(oldname))
            {
                if (!CheckTagExists(newname))
                {
                    Debug.Assert(GetTag(oldname) != null);
                    Tag tag = GetTag(oldname);
                    tag.RenameTag(newname, updated.CurrentTimeLong);
                    UpdateTaggingProfileDate(updated.CurrentTimeLong);
                    TaggingHelper.Logging(LogMessage.TAG_RENAMED, oldname, newname);
                }
                else
                {
                    TaggingHelper.Logging(LogMessage.TAG_ALREADY_EXISTS, newname);
                    throw new TagAlreadyExistsException(newname);
                }
            }
            else
            {
                TaggingHelper.Logging(LogMessage.TAG_NOT_FOUND, oldname);
                throw new TagNotFoundException(oldname);
            }
        }

        /// <summary>
        /// Remove the Tag of tagname
        /// </summary>
        /// <param name="tagname">The name of the Tag to be removed</param>
        /// <returns>The Tag that is removed successfully, else raise TagNotFoundException</returns>
        #region deprecated
        //public Tag RemoveTag(string tagname)
        //{
        //    CurrentTime updated = new CurrentTime();
        //    Tag toRemove;
        //    if (CheckTagExists(tagname))
        //    {
        //        toRemove = GetTag(tagname);
        //        _taggingProfile.TagList.Remove(toRemove);
        //        UpdateTaggingProfileDate(updated.CurrentTimeLong);
        //        TaggingHelper.Logging(LogMessage.TAG_REMOVED, tagname);
        //        return toRemove;
        //    }
        //    else
        //    {
        //        TaggingHelper.Logging(LogMessage.TAG_NOT_FOUND, tagname);
        //        throw new TagNotFoundException(tagname);
        //    }
        //}
        #endregion
        public Tag DeleteTag(string tagname)
        {
            CurrentTime updated = new CurrentTime();
            Tag toRemove = GetTag(tagname);
            if (toRemove != null)
            {
                if (toRemove.IsDeleted)
                {
                    TaggingHelper.Logging(LogMessage.TAG_NOT_FOUND, tagname);
                    throw new TagNotFoundException(tagname);
                }
                else
                {
                    toRemove.IsDeleted = true;
                    toRemove.DeletedDate = updated.CurrentTimeLong;
                    toRemove.LastUpdated = updated.CurrentTimeLong;
                    toRemove.RemoveAllPaths();
                    UpdateTaggingProfileDate(updated.CurrentTimeLong);
                    TaggingHelper.Logging(LogMessage.TAG_REMOVED, tagname);
                    return toRemove;
                }
            }
            else
            {
                TaggingHelper.Logging(LogMessage.TAG_NOT_FOUND, tagname);
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
            CurrentTime updated = new CurrentTime();
            Tag tag = GetTag(tagname);
            if (tag == null)
            {
                tag = new Tag(tagname, updated.CurrentTimeLong);
            }
            Debug.Assert(tag != null);
            if (!tag.Contains(path))
            {
                if (!TaggingHelper.CheckRecursiveDirectory(tag, path))
                {
                    tag.AddPath(path, updated.CurrentTimeLong);
                    AddTag(tag);
                    UpdateTaggingProfileDate(updated.CurrentTimeLong);
                    TaggingHelper.Logging(LogMessage.FOLDER_TAGGED, path, tagname);
                    return tag;
                }
                else
                {
                    TaggingHelper.Logging(LogMessage.RECURSIVE_DIRECTORY, path);
                    throw new RecursiveDirectoryException(path, tagname);
                }
            }
            else
            {
                TaggingHelper.Logging(LogMessage.PATH_ALREADY_EXISTS_IN_TAG, path, tagname);
                throw new PathAlreadyExistsException(path);
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
            CurrentTime updated = new CurrentTime();
            Tag tag = RetrieveTag(tagname);
            if (tag != null)
            {
                if (tag.RemovePath(path, updated.CurrentTimeLong))
                {
                    UpdateTaggingProfileDate(updated.CurrentTimeLong);
                    TaggingHelper.Logging(LogMessage.FOLDER_UNTAGGED, path, tagname);
                    return 1;
                }
                else
                {
                    TaggingHelper.Logging(LogMessage.FOLDER_NOT_UNTAGGED, path, tagname);
                    return 0;
                }
            }
            else
            {
                TaggingHelper.Logging(LogMessage.TAG_NOT_FOUND, tagname);
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
            int noOfPath = 0;
            CurrentTime updated = new CurrentTime();
            foreach (Tag tag in _taggingProfile.TagList)
            {
                if (tag.Contains(path))
                {
                    tag.RemovePath(path, updated.CurrentTimeLong);
                    UpdateTaggingProfileDate(updated.CurrentTimeLong);
                    noOfPath++;
                }
            }
            return noOfPath;
        }
        public void UpdateFilter(string tagname, List<Filter> newFilterList)
        {
            Tag tag = GetTag(tagname);
            if (tag == null)
            {
                throw new TagNotFoundException(tagname);
            }
            tag.Filters = newFilterList;

        }
        /// <summary>
        /// Rename a path in all the Tags it is tagged to
        /// </summary>
        /// <param name="oldPath">The original path of the folder</param>
        /// <param name="newPath">The new path of the folder</param>
        public void RenameFolder(string oldPath, string newPath)
        {
            foreach (Tag tag in _taggingProfile.TagList)
            {
                if (tag.Contains(oldPath))
                {
                    Debug.Assert(!tag.Contains(newPath));
                    if (!tag.Contains(newPath))
                    {
                        CurrentTime updated = new CurrentTime();
                        tag.RenamePath(oldPath, newPath, updated.CurrentTimeLong);
                        UpdateTaggingProfileDate(updated.CurrentTimeLong);
                        TaggingHelper.Logging(LogMessage.FOLDER_RENAMED, oldPath, newPath);
                    }
                }
            }
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
            if (getdeleted)
            {
                return _taggingProfile.TagList;
            }
            else
            {
                List<Tag> pathList = new List<Tag>();
                foreach (Tag tag in _taggingProfile.TagList)
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
        /// Retrieve a list of Tags where a given path is tagged to
        /// </summary>
        /// <param name="path">The path to find the Tags it is tagged to</param>
        /// <returns>The list of Tags containing the given path</returns>
        public List<Tag> RetrieveTagByPath(string path)
        {
            List<Tag> tagList = new List<Tag>();
            foreach (Tag tag in _taggingProfile.TagList)
            {
                if (tag.Contains(path))
                {
                    tagList.Add(tag);
                }
            }
            return tagList;
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
                    if (path.LogicalDriveId.Equals(logicalid))
                    {
                        if (!pathList.Contains(path.Path))
                        {
                            pathList.Add(path.Path);
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
                        if (!pathList.Contains(p.Path) && !p.Path.Equals(folderPath))
                        {
                            pathList.Add(p.Path);
                        }
                    }
                }
            }
            List<Tag> matchingTag = RetrieveTagById(logicalid);
            foreach (Tag tag in matchingTag)
            {
                string appendedPath;
                string trailingPath = tag.FindMatchedParentDirectory(folderPath, true);
                if (trailingPath != null)
                {
                    foreach (TaggedPath p in tag.FilteredPathList)
                    {
                        appendedPath = p.Append(trailingPath);
                        if (!pathList.Contains(appendedPath) && !appendedPath.Equals(folderPath))
                        {
                            pathList.Add(appendedPath);
                        }
                    }
                }
            }
            return pathList;
        }

        /// <summary>
        /// Find a list of paths of files which share the same parent directories as filePath
        /// </summary>
        /// <param name="filePath">The path to search</param>
        /// <returns>The list of similar paths</returns>
        public List<string> FindSimilarPathForFile(string filePath)
        {
            string logicalid = TaggingHelper.GetLogicalID(filePath);
            List<string> pathList = new List<string>();
            List<Tag> matchingTag = RetrieveTagById(logicalid);
            foreach (Tag tag in matchingTag)
            {
                string appendedPath;
                string trailingPath = tag.FindMatchedParentDirectory(filePath, false);
                if (trailingPath != null)
                {
                    foreach (TaggedPath p in tag.FilteredPathList)
                    {
                        appendedPath = p.Append(trailingPath);
                        if (!pathList.Contains(appendedPath) && !appendedPath.Equals(filePath))
                        {
                            pathList.Add(appendedPath);
                        }
                    }
                }
            }
            return pathList;
        }

        /// <summary>
        /// Retrieve the list of parent directories of a given path
        /// </summary>
        /// <param name="path">The path of which the parent directories to be found</param>
        /// <returns>The list of parent directories</returns>
        public List<string> RetrieveParentByPath(string path)
        {
            List<string> parentPathList = new List<string>();
            foreach (Tag tag in _taggingProfile.TagList)
            {
                foreach (TaggedPath p in tag.FilteredPathList)
                {
                    if (path.StartsWith(p.Path))
                    {
                        if (!path.Equals(p.Path))
                        {
                            if (!parentPathList.Contains(p.Path))
                            {
                                parentPathList.Add(p.Path);
                                break;
                            }
                        }
                    }
                }
            }
            return parentPathList;
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
            TaggingXMLHelper.SaveTo(_taggingProfile, savedLocation);
        }
        #endregion

        #region private methods implementations
        #region completed
        private void UpdateTaggingProfileDate(long created)
        {
            _taggingProfile.LastUpdated = created;
        }

        private Tag RetrieveTag(string tagname, bool create, bool getdeleted, long lastupdated)
        {
            Tag tag = GetTag(tagname);
            if (tag != null)
            {
                if (tag.IsDeleted == true)
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

        private void AddTag(Tag tag)
        {
            if (!CheckTagExists(tag.TagName))
            {
                _taggingProfile.TagList.Add(tag);
            }
        }

        private Tag GetTag(string tagname)
        {
            foreach (Tag tag in _taggingProfile.TagList)
            {
                if (tag.TagName.ToLower().Equals(tagname.ToLower()))
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
                if (tag.TagName.ToLower().Equals(tagname.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        private List<Tag> RetrieveTagById(string logicalid)
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
