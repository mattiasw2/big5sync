using System.Collections.Generic;
using Syncless.Core;
using Syncless.Notification;
namespace Syncless.Tagging
{
    /// <summary>
    /// TagMerger class performs merging of several tagging profiles which have the same name.
    /// </summary>
    public static class TagMerger
    {
        /// <summary>
        /// Merges two tagging profiles with the same profile name by merging the tags in each profile
        /// </summary>
        /// <param name="currentProfile">The <see cref="TaggingProfile">TaggingProfile</see> object that
        /// represents the current tagging profile</param>
        /// <param name="newProfile">The <see cref="TaggingProfile">TaggingProfile</see> object that
        /// represents the new tagging profile</param>
        /// <returns>the number of tags merged, -1 if the profiles are null or they have different
        /// profile name</returns>
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
            newTagList.AddRange(newProfile.ReadOnlyTagList);
            List<Tag> monitorList = new List<Tag>();
            foreach (Tag newTag in newTagList)//handles the new tag from new profile
            {
                Tag oldTag = currentProfile.FindTag(newTag.TagName);
                if (oldTag != null)
                {
                    bool merge = MergeTag(oldTag, newTag);
                    if (merge)
                    {
                        updateCount++;
                    }
                }
                else
                {
                    TaggingLayer.Instance.AddTag(newTag);
                    monitorList.Add(newTag);
                    updateCount++;
                }
            }

            foreach (Tag tag in monitorList)//start monitoring each new tag
            {
                ServiceLocator.LogicLayerNotificationQueue().Enqueue(new AddTagNotification(tag));
                //SystemLogicLayer.Instance.StartMonitorTag(tag, tag.IsDeleted);
            }

            return updateCount;
        }

        /// <summary>
        /// Merges two tags with the same tag name by merging the tagged paths in each tag
        /// </summary>
        /// <param name="current">The <see cref="Tag">Tag</see> object that represents the current tag</param>
        /// <param name="newTag">The <see cref="Tag">Tag</see> object that represents the new tag</param>
        /// <returns>true if the tags are merged, false if the tags have different tag name or 
        /// same last updated date or different last updated date</returns>
        private static bool MergeTag(Tag current, Tag newTag)
        {
            if (!newTag.TagName.ToLower().Equals(current.TagName.ToLower()))
            {
                //Since Tag name is different , do not merge.
                //Should not Happen.
                return false;
            }
            if (newTag.LastUpdatedDate == current.LastUpdatedDate)
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
                    if (newTag.CreatedDate == current.CreatedDate)
                    {
                        ServiceLocator.LogicLayerNotificationQueue().Enqueue(new RemoveTagNotification(newTag));
                        //SystemLogicLayer.Instance.DeleteTag(newTag.TagName);
                        return true;
                    }
                    return false;
                }
                //for each taggedPath found in the new Tag.
                //if the path is not found , just create
                //if the path is found , attempt to merge.
                if (current.IsDeleted && !newTag.IsDeleted)
                {
                    if (newTag.CreatedDate > current.DeletedDate)
                    {
                        ServiceLocator.LogicLayerNotificationQueue().Enqueue(new AddTagNotification(newTag));
                        return true;
                        //SystemLogicLayer.Instance.AddTag(newTag);
                    }
                }
                foreach (TaggedPath newPath in newTag.UnfilteredPathList)
                {
                    TaggedPath currentPath = current.FindPath(newPath.PathName, false);
                    if (currentPath == null)
                    {
                        ServiceLocator.LogicLayerNotificationQueue().Enqueue(new MonitorPathNotification(current,newPath));
                        //SystemLogicLayer.Instance.AddTagPath(current, newPath);
                        current.AddPath(newPath);
                    }
                    else
                    {
                        //update only if the new path is more updated than the current path
                        if (currentPath.LastUpdatedDate <= newPath.LastUpdatedDate)
                        {
                            //if the path is delete in the new tag but not in the old tag
                            if (newPath.IsDeleted && !currentPath.IsDeleted)
                            {
                                if (newPath.DeletedDate > currentPath.CreatedDate)
                                {
                                    current.RemovePath(newPath);
                                    ServiceLocator.LogicLayerNotificationQueue().Enqueue(new UnMonitorPathNotification(current,newPath));
                                    //SystemLogicLayer.Instance.RemoveTagPath(current, newPath);
                                }
                            }
                            else if (!newPath.IsDeleted && currentPath.IsDeleted)
                            {
                                if (newPath.CreatedDate > currentPath.DeletedDate)
                                {
                                    //a new path is created in the new tag but is deleted in the old tag.
                                    current.AddPath(newPath);
                                    ServiceLocator.LogicLayerNotificationQueue().Enqueue(new MonitorPathNotification(current,newPath));
                                    //SystemLogicLayer.Instance.AddTagPath(current, newPath);
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
