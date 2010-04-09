using System.Collections.Generic;
using System.IO;
using Syncless.Helper;
using Syncless.Tagging.Exceptions;
using System.Diagnostics;
using Syncless.Filters;
using Syncless.Logging;

namespace Syncless.Tagging
{
    /// <summary>
    /// TaggingLayer class is the main logic controller of the Tagging namespace
    /// </summary>
    public class TaggingLayer
    {
        /// <summary>
        /// The string value for the relative path to save the tagging.xml in a .syncless folder
        /// </summary>
        public const string RELATIVE_TAGGING_SAVE_PATH = ".syncless\\tagging.xml";

        /// <summary>
        /// The string value for the relative path to save the tagging.xml in the root folder
        /// </summary>
        public const string RELATIVE_TAGGING_ROOT_SAVE_PATH = "tagging.xml";

        #region attributes
        private static TaggingLayer _instance;
        private TaggingProfile _taggingProfile;

        /// <summary>
        /// Gets a singleton instance of the TaggingLayer object
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
        /// Gets or sets the tagging profile
        /// </summary>
        public TaggingProfile TaggingProfile
        {
            get { return _taggingProfile; }
            set { _taggingProfile = value; }
        }

        /// <summary>
        /// Gets an immutable list of tag list
        /// </summary>
        public List<Tag> TagList
        {
            get { return _taggingProfile.ReadOnlyTagList; }
        }

        /// <summary>
        /// Gets a clone of the list of Tag objects which are not set as deleted
        /// </summary>
        public List<Tag> FilteredTagList
        {
            get
            {
                List<Tag> filteredTagList = new List<Tag>();
                foreach (Tag tag in _taggingProfile.ReadOnlyTagList)
                {
                    if (!tag.IsDeleted)
                    {
                        filteredTagList.Add(tag);
                    }
                }
                return filteredTagList;
            }
        }

        /// <summary>
        /// Gets a clone of the list of tags
        /// </summary>
        public List<Tag> UnfilteredTagList
        {
            get
            {
                List<Tag> allTagList = new List<Tag>();
                foreach (Tag tag in _taggingProfile.ReadOnlyTagList)
                {
                    allTagList.Add(tag);
                }
                return allTagList;
            }
        }
        #endregion

        /// <summary>
        /// Creates a new TaggingLayer object
        /// </summary>
        private TaggingLayer()
        {
            _taggingProfile = new TaggingProfile(TaggingHelper.GetCurrentTime());
        }

        /// <summary>
        /// Initializes tagging profile object. If a tagging.xml file has already been created, 
        /// loads the information from the file; otherwise, instantiates a new tagging profile object.
        /// </summary>
        /// <param name="paths">The list of strings which represent the paths of the tagging.xml 
        /// files to be loaded from</param>
        /// <remarks>paths[0] is always the root</remarks>
        public void Init(List<string> paths)
        {
            string profileFilePath = paths[0];
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
        /// Merges the profile that is loaded from the path that is passed as parameter to the current tagging
        /// profile
        /// </summary>
        /// <param name="path">The string value that represents the path of the xml file to load the new
        /// profile from</param>
        /// <returns>true if merging is successful; otherwise, false</returns>
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
        /// Creates a tag with a name that is given by the tag name that is passed as parameter
        /// </summary>
        /// <param name="tagname">The string value that represents the name that is to be given to the tag
        /// </param>
        /// <returns>the created tag</returns>
        /// <exception cref="Syncless.Tagging.Exceptions.TagAlreadyExistsException">thrown if the 
        /// name that is passed as parameter is already used by another tag in the existing list 
        /// of tags</exception>
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
        /// Sets the name of a tag that is the same as the old name to the new name that is passed as parameter
        /// </summary>
        /// <param name="oldname">The string value that represents the old name of a tag</param>
        /// <param name="newname">The string value that represents the new name to be given to the tag</param>
        /// <exception cref="Syncless.Tagging.Exceptions.TagAlreadyExistsException">thrown if the new name
        /// that is passed as parameter is already used by another tag in the existing list of tags</exception>
        /// <exception cref="Syncless.Tagging.Exceptions.TagNotFoundException">thrown if the old name that is
        /// passed as parameter is not used by any tag in the existing list of tags</exception>
        public void RenameTag(string oldname, string newname)
        {
            int result = _taggingProfile.RenameTag(oldname, newname);
            switch (result)
            {
                case 0:
                    break;
                case 1:
                    throw new TagAlreadyExistsException(newname);
                case 2:
                    throw new TagNotFoundException(oldname);
            }
        }

        /// <summary>
        /// Removes the tag whose tag name is the same as the tag name that is passed as parameter
        /// </summary>
        /// <param name="tagname">The string value that represents the name that is to be used to retrieve
        /// the tag to be deleted</param>
        /// <returns>the tag that is deleted</returns>
        /// <exception cref="Syncless.Tagging.Exceptions.TagNotFoundException">thrown if the name that 
        /// is passed as parameter is not used by any tag in the existing list of tags</exception>
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
        /// Tags a folder path to a tag with name same as the tag name that is passed as parameter
        /// </summary>
        /// <param name="path">The string value that represents the path of the folder to be tagged</param>
        /// <param name="tagname">The string value that represents the name of the tag the folder path is
        /// to be tagged to</param>
        /// <returns>the tag where the folder path is tagged to</returns>
        /// <exception cref="Syncless.Tagging.Exceptions.PathAlreadyExistsException">thrown if the folder 
        /// path that is passed as parameter is already tagged to the tag</exception>
        /// <exception cref="Syncless.Tagging.Exceptions.RecursiveDirectoryException">thrown if the folder 
        /// path that is passed as parameter is a parent path or a child path of another path that is 
        /// already tagged to the tag</exception>
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
        /// Untags a folder path from a tag with name same as the tag name that is passed as parameter
        /// </summary>
        /// <param name="path">The string value that represents the path of the folder to be untagged</param>
        /// <param name="tagname">The string value that represents the name of the tag the folder path is
        /// to be untagged from</param>
        /// <returns>1 if the path is removed, 0 if the path is not found in the Tag</returns>
        /// <exception cref="Syncless.Tagging.Exceptions.TagNotFoundException">thrown if the name that is 
        /// passed as parameter is not used by any tag in the existing list of tags</exception>
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
        /// Untags a folder path from all tags where the folder path is tagged to
        /// </summary>
        /// <param name="path">The string value that represents the path of the folder to be untagged</param>
        /// <returns>the number of tags the path is untagged from</returns>
        public int UntagFolder(string path)
        {
            return _taggingProfile.UntagFolder(path);
        }

        /// <summary>
        /// Sets the name of a folder path name that is the same as the old path name to the new path 
        /// name that is passed as parameter
        /// </summary>
        /// <param name="oldPath">The string value that represents the old name of a folder path</param>
        /// <param name="newPath">The string value that represents the new name of a folder path</param>
        /// <returns>the number of folder paths whose old name is replaced by the new name</returns>
        public int RenameFolder(string oldPath, string newPath)
        {
            return _taggingProfile.RenameFolder(oldPath, newPath);
        }

        /// <summary>
        /// Updates the list of filters for a tag with name same as the tag name that is passed as parameter
        /// </summary>
        /// <param name="tagname">The string value that represents the name that is to be used to retrieve
        /// the tag</param>
        /// <param name="newFilterList">The list of filters that is to be updated to the tag</param>
        /// <exception cref="Syncless.Tagging.Exceptions.TagNotFoundException">thrown if the name that is 
        /// passed as parameter is not used by any tag in the existing list of tags</exception>
        public void UpdateFilter(string tagname, List<Filter> newFilterList)
        {
            if (!_taggingProfile.UpdateFilter(tagname, newFilterList))
            {
                throw new TagNotFoundException(tagname);
            }
        }

        #region retrieve tag methods
        /// <summary>
        /// Gets the tag whose name is same as the tag name that is passed as parameter
        /// </summary>
        /// <param name="tagname">The string value that represents the name of the tag to be retrieved</param>
        /// <returns>the tag that is to be retrieved if it is found; otherwise null</returns>
        public Tag RetrieveTag(string tagname)
        {
            return RetrieveTag(tagname, false, 0);
        }

        /// <summary>
        /// Gets the tag whose name is same as the tag name that is passed as parameter
        /// </summary>
        /// <param name="tagname">The string value that represents the name of the tag to be retrieved</param>
        /// <param name="getdeleted">The boolean value that represents whether to return the tag if it set
        /// as deleted</param>
        /// <returns>the tag that is to be retrieved if it is found; otherwise null</returns>
        public Tag RetrieveTag(string tagname, bool getdeleted)
        {
            return RetrieveTag(tagname, false, getdeleted, 0);
        }

        /// <summary>
        /// Gets the list of all tags
        /// </summary>
        /// <param name="getdeleted">The boolean value that represents whether to return the tag if it set
        /// as deleted</param>
        /// <returns>the list of all tags</returns>
        public List<Tag> RetrieveAllTags(bool getdeleted)
        {
            return _taggingProfile.RetrieveAllTags(getdeleted);
        }

        /// <summary>
        /// Gets a list of tags where a path, that is passed as parameter, is tagged to
        /// </summary>
        /// <param name="path">The string value that represents the path of the folder to be used to
        /// retrieve a list of tags</param>
        /// <returns>the list of tags where the path is tagged to</returns>
        public List<Tag> RetrieveTagByPath(string path)
        {
            return _taggingProfile.RetrieveTagsByPath(path);
        }

        /// <summary>
        /// Gets a list of tags where paths, whose logical ID is same as logical ID that is passed as parameter,
        /// are tagged to
        /// </summary>
        /// <param name="logicalid">The string value that represents the logical ID to be used to retrieve
        /// the list of tags</param>
        /// <returns>the list of tags containing tagged paths whose logical ID is same as the logical 
        /// ID passed as parameter</returns>
        public List<Tag> RetrieveTagByLogicalId(string logicalid)
        {
            bool found;
            List<Tag> tagList = new List<Tag>();
            foreach (Tag tag in _taggingProfile.ReadOnlyTagList)
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
        /// Gets a filtered list of tags containing tagged paths whose logical ID is same as logical ID that is 
        /// passed as parameter
        /// </summary>
        /// <param name="logicalid">The string value that represents the logical ID to be used to retrieve
        /// the list of tags</param>
        /// <returns>the list of tags containing tagged paths whose logical ID is same as the logical 
        /// ID passed as parameter</returns>
        public List<Tag> RetrieveFilteredTagByLogicalId(string logicalid)
        {
            bool found;
            List<Tag> tagList = new List<Tag>();
            foreach (Tag tag in _taggingProfile.ReadOnlyTagList)
            {
                if (!tag.IsDeleted)
                {
                    found = CheckID(tag, logicalid);
                    if (found)
                    {
                        tagList.Add(tag);
                    }
                }
            }
            return tagList;
        }

        /// <summary>
        /// Gets a list of tags containing tagged paths who are parent paths of the path that is passed
        /// as parameter
        /// </summary>
        /// <param name="path">The string value that represents the path of the folder to be used to
        /// retrieve a list of tags</param>
        /// <returns>the list of tags containing the tagged paths who are parent paths of the path
        /// that is passed as parameter</returns>
        public List<Tag> RetrieveParentTagByPath(string path)
        {
            List<Tag> parentPathList = new List<Tag>();
            foreach (Tag tag in _taggingProfile.ReadOnlyTagList)
            {
                foreach (TaggedPath p in tag.FilteredPathList)
                {
                    if (PathHelper.StartsWithIgnoreCase(path, p.PathName) && !PathHelper.EqualsIgnoreCase(path, p.PathName))
                    {
                        parentPathList.Add(tag);
                        break;
                    }
                }
            }
            return parentPathList;
        }
        #endregion

        #region retrieve path methods
        /// <summary>
        /// Gets a list of paths whose logical ID is same as the logical ID that is passed as parameter
        /// </summary>
        /// <param name="logicalid">The string value that represents the logical ID to be used to retrieve
        /// the list of paths</param>
        /// <returns>the list of paths whose logical ID is same as the logical ID passed as parameter</returns>
        public List<string> RetrievePathByLogicalId(string logicalid)
        {
            List<string> pathList = new List<string>();
            List<Tag> tagList = RetrieveTagByLogicalId(logicalid);
            foreach (Tag tag in tagList)
            {
                foreach (TaggedPath path in tag.FilteredPathList)
                {
                    if (path.LogicalDriveId.Equals(logicalid) && !PathHelper.ContainsIgnoreCase(pathList, path.PathName))
                    {
                        pathList.Add(path.PathName);
                    }
                }
            }
            return pathList;
        }

        /// <summary>
        /// Gets a list of folder paths or sub-folder paths which are tagged to the same tag that the path,
        /// that is passed as parameter, is tagged to
        /// </summary>
        /// <param name="folderPath">The string value that represents the path of the folder to be used 
        /// to retrieve a list of paths</param>
        /// <returns>the list of folder paths or sub-folder paths which are tagged to the same tag
        /// that the path, that is passed as parameter, is tagged to</returns>
        /// <remarks>Example: TagA - D:\A\, E:\B\C\
        ///          TagB - D:\A\, F:\D\E\G\
        ///          TagC - E:\G\, F:\H\
        ///          Given path D:\A\H\J\
        ///          Should return E:\B\C\H\J\ from TagA, F:\D\E\G\H\J\ from TagB
        /// </remarks>
        public List<string> FindSimilarPathForFolder(string folderPath)
        {
            /**
             * for all tags that contain tagged path same as folder path that is passed as parameter,
             * add all the other tagged paths in the tag to the list of paths
            **/
            List<string> pathList = new List<string>();
            pathList.AddRange(GetAllPathStringInSameTag(folderPath));

            /** 
             * retrieve tags which have tagged path with logical ID that is same as the folder path 
             * passed as parameter
             **/
            string logicalid = TaggingHelper.GetLogicalID(folderPath);
            List<Tag> matchingTag = RetrieveTagByLogicalId(logicalid);
            pathList.AddRange(GetAllAppendedPathStringInSameTag(matchingTag, folderPath));

            return pathList;
        }

        /// <summary>
        /// Gets a list of paths which are the ancestor paths of the path that is passed as parameter
        /// </summary>
        /// <param name="path">The string value that represents the path of the folder to be used 
        /// to retrieve a list of ancestor paths</param>
        /// <returns>the list of folder paths which are the ancestor paths of the path that is passed as
        /// parameter</returns>
        public List<string> RetrieveAncestors(string path)
        {
            List<string> ancestors = new List<string>();
            foreach (Tag tag in _taggingProfile.ReadOnlyTagList)
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
        /// Gets a list of paths which are the descendant paths of the path that is passed as parameter
        /// </summary>
        /// <param name="path">The string value that represents the path of the folder to be used 
        /// to retrieve a list of descendant paths</param>
        /// <returns>the list of folder paths which are the descendant paths of the path that is passed as
        /// parameter</returns>
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
        /// Gets a filtered list of all paths that are tagged to all tags
        /// </summary>
        /// <returns>the filtered list of all paths that are tagged to all tags</returns>
        public List<string> GetAllPaths()
        {
            return _taggingProfile.AllFilteredTaggedPathList;
        }
        #endregion

        /// <summary>
        /// Determines whether the logical ID exists in any tagged path in some tags
        /// </summary>
        /// <param name="logicalid">The string value that represents the logical ID</param>
        /// <returns>true if the logical ID exists in any tagged path in some tags; otherwise false</returns>
        public bool CheckIDExists(string logicalid)
        {
            foreach (Tag tag in _taggingProfile.ReadOnlyTagList)
            {
                if (CheckID(tag, logicalid))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Saves the tagging profile to a xml file
        /// </summary>
        /// <param name="savedLocation">The list of strings that represent the list of paths that the 
        /// xml file is to be saved to</param>
        public void SaveTo(List<string> savedLocation)
        {
            TaggingXMLHelper.SaveToLocations(_taggingProfile, savedLocation);
        }

        /// <summary>
        /// Appends the current tagging profile to the tagging.xml saved in the list of locations passed as
        /// parameter
        /// </summary>
        /// <param name="savedLocation">The list of locations containing tagging.xml where current profile
        /// is to be saved to</param>
        public void AppendProfile(List<string> savedLocation)
        {
            TaggingXMLHelper.AppendProfile(_taggingProfile, savedLocation);
        }

        #region for Merger
        /// <summary>
        /// Adds a tag to the current tagging profile
        /// </summary>
        /// <param name="tag">The Tag object that represents the tag to be added</param>
        /// <remarks>Used for merging tag objects from several tagging profiles.</remarks>
        public void AddTag(Tag tag)
        {
            _taggingProfile.AddTag(tag);
        }
        #endregion
        #endregion

        #region private methods implementations
        /// <summary>
        /// Gets the tag with name that is same as the tag name that is passed as parameter
        /// </summary>
        /// <param name="tagname">The string value that represents the tag name of the tag 
        /// to be retrieved</param>
        /// <param name="create">The boolean value that indicates whether to create the tag if
        /// it does not exist</param>
        /// <param name="getdeleted">The boolean value that indicates whether to retrieve tag
        /// which is set as deleted</param>
        /// <param name="lastupdated">The long value that represents the current date time to be
        /// assigned to the tag if it is newly created</param>
        /// <returns>the tag if it is found or created; otherwise, null</returns>
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

        /// <summary>
        /// Gets the tag with name that is same as the tag name that is passed as parameter
        /// </summary>
        /// <param name="tagname">The string value that represents the tag name of the tag 
        /// to be retrieved</param>
        /// <param name="create">The boolean value that indicates whether to create the tag if
        /// it does not exist</param>
        /// <param name="lastupdated">The long value that represents the current date time to be
        /// assigned to the tag if it is newly created</param>
        /// <returns>the tag if it is found or created; otherwise, null</returns>
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

        /// <summary>
        /// Gets the tag with name that is same as the tag name that is passed as parameter
        /// </summary>
        /// <param name="tagname">The string value that represents the tag name of the tag 
        /// to be retrieved</param>
        /// <returns>the tag if it is found; otherwise, null</returns>
        private Tag GetTag(string tagname)
        {
            foreach (Tag tag in _taggingProfile.ReadOnlyTagList)
            {
                if (TaggingHelper.FormatTagName(tag.TagName).Equals(TaggingHelper.FormatTagName(tagname)))
                {
                    return tag;
                }
            }
            return null;
        }

        /// <summary>
        /// Determines whether a tag with name that is same as the tag name that is passed as parameter
        /// exists in the list of tags
        /// </summary>
        /// <param name="tagname">The string value that represents the tag name of the tag 
        /// to be checked for existence</param>
        /// <returns>true if the tag exists; otherwise, false</returns>
        private bool CheckTagExists(string tagname)
        {
            foreach (Tag tag in _taggingProfile.ReadOnlyTagList)
            {
                if (TaggingHelper.FormatTagName(tag.TagName).Equals(TaggingHelper.FormatTagName(tagname)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the logical ID exists in any tagged path in the tag that is passed as parameter
        /// </summary>
        /// <param name="tag">The Tag object that represents the tag to be checked</param>
        /// <param name="ID">The string value that represents the logical ID</param>
        /// <returns>true if the logical ID exists in any tagged path in the tag; otherwise, false</returns>
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

        private List<string> GetAllPathStringInSameTag(string path)
        {
            List<string> pathList = new List<string>();
            foreach (Tag tag in _taggingProfile.ReadOnlyTagList)
            {
                if (tag.Contains(path))
                {
                    foreach (TaggedPath p in tag.FilteredPathList)
                    {
                        if (!PathHelper.ContainsIgnoreCase(pathList, p.PathName) && !PathHelper.EqualsIgnoreCase(p.PathName, path))
                        {
                            pathList.Add(p.PathName);
                        }
                    }
                }
            }
            return pathList;
        }

        private List<string> GetAllAppendedPathStringInSameTag(List<Tag> taglist, string path)
        {
            List<string> pathList = new List<string>();
            foreach (Tag tag in taglist)
            {
                /**
                 * extract the trailing end of the folder path passed as parameter
                 * eg. folder path is D:\A\B\C\D\
                 *     tag contains tagged paths D:\A\B\ and E:\F\B\
                 *     trailing path will be C\D\
                 **/
                string trailingPath = tag.CreateTrailingPath(path, true);

                /** 
                 * if trailing path is extracted, append the trailing path to all tagged paths in the tag
                 * eg. tag contains tagged path D:\A\B\ and E:\F\B\, append C\D\ to E:\F\B\
                 *     appended path will be E:\F\B\C\D\
                 **/
                string appendedPath;
                if (trailingPath != null)
                {
                    foreach (TaggedPath p in tag.FilteredPathList)
                    {
                        appendedPath = p.Append(trailingPath);
                        if (!PathHelper.ContainsIgnoreCase(pathList, appendedPath) && !PathHelper.EqualsIgnoreCase(appendedPath, path))
                        {
                            pathList.Add(appendedPath);
                        }
                    }
                }
            }
            return pathList;
        }
        #endregion
    }
}
