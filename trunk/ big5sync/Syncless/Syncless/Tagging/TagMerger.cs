using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Core;
namespace Syncless.Tagging
{
    public static class TagMerger
    {

        public static int MergeProfile(TaggingProfile currentProfile, TaggingProfile newProfile)
        {
            //the number of updates/
            int updateCount = 0;
            //if any of the profile is null.
            if (currentProfile == null || newProfile == null)
            {                
                //should not happen. but take care of it anyway.
                return -1;
            }
            //if profile have a different name do not merge
            if (!currentProfile.ProfileName.Equals(newProfile.ProfileName))
            {
                //might want to throw a exception here
                return -1;
            }
            //list of tags from the new profile.
            List<Tag> newTagList = new List<Tag>();
            newTagList.AddRange(newProfile.TagList);
            //for each tag in the current profile , check for its existence in the new profile.
            foreach (Tag tag in currentProfile.TagList)
            {
                Tag newTag = newProfile.FindTag(tag.TagName);
                if (newTag == null)
                {
                    continue;
                    // a new tag in current profile that is not in the new profile.
                }
                newTagList.Remove(newTag);
                //if the tag exist , merge it.
                bool merge = MergeTag(tag, newTag);
                if (merge)
                {
                    updateCount++;
                }
            }
            foreach (Tag tag in newTagList)//handles the new tag from new profile
            {
                
                Tag oldTag = currentProfile.FindTag(tag.TagName);
                if (oldTag != null)
                {
                    //already exist in currentProfile.
                    //should not happen, but handle it anyway.
                    continue;                    
                }
                TaggingLayer.Instance.AddTag(tag);
                updateCount++;
            }
            
            foreach (Tag tag in newTagList)//start monitoring each new tag
            {
                SystemLogicLayer.Instance.MonitorTag(tag, tag.IsDeleted);
            }

            return updateCount;
        }

        private static bool MergeTag(Tag current, Tag newTag)
        {
            if (!newTag.TagName.Equals(current.TagName))
            {
                //Since Tag name is different , do not merge.
                //Should not Happen.
                return false;
            }
            if (newTag.LastUpdated == current.LastUpdated)
            {
                //Since Tag updated time is same , shall not do anything
                return false;
            }
            else
            {//Since time different , merge.
                //if new Tag is deleted and current is not
                if (newTag.IsDeleted && !current.IsDeleted)
                {
                    
                    //check the creation date is the same , then delete
                    if (newTag.Created == current.Created)
                    {
                        SystemLogicLayer.Instance.DeleteTag(newTag.TagName);
                    }
                }
                //for each taggedPath found in the new Tag.
                //if the path is not found , just create
                //if the path is found , attempt to merge.
                foreach (TaggedPath newPath in newTag.UnfilteredPathList)
                {
                    TaggedPath currentPath = current.FindPath(newPath.Path, false);
                    if (currentPath == null)
                    {
                        SystemLogicLayer.Instance.AddTagPath(current, newPath);
                    }
                    //if updated time is the same, does not merge
                    if (currentPath.LastUpdated == newPath.LastUpdated)
                    {
                        continue;
                    }                    
                    else
                    {
                        //update only if the new path is more updated than the current path
                        if (currentPath.LastUpdated < newPath.LastUpdated)
                        {
                            //if the path is delete in the new tag but not in the old tag
                            if (newPath.IsDeleted && !currentPath.IsDeleted)
                            {
                                if (newPath.DeletedDate > currentPath.Created)
                                {
                                    current.RemovePath(newPath);
                                    SystemLogicLayer.Instance.RemoveTagPath(current, newPath);                                    
                                }
                            }
                            else if (!newPath.IsDeleted && currentPath.IsDeleted)
                            {
                                if (newPath.Created > currentPath.DeletedDate)
                                {
                                    //a new path is created in the new tag but is deleted in the old tag.
                                    current.AddPath(newPath);
                                    SystemLogicLayer.Instance.AddTagPath(current, newPath);                                    
                                }                               
                            }
                        }
                    }
                }
            }
            return true;


        }
    }
}
