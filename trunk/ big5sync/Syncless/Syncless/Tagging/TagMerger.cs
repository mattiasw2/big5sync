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
            int updateCount = 0;
            if (!currentProfile.ProfileName.Equals(newProfile.ProfileName))
            {
                return -1;
            }
            List<Tag> newTagList = new List<Tag>();
            newTagList.AddRange(newProfile.TagList);
            foreach (Tag tag in currentProfile.TagList)
            {
                Tag newTag = newProfile.FindTag(tag.TagName);
                if (newTag == null)
                {
                    continue;
                    // a new tag in current profile that is not in the new profile.
                }
                newTagList.Remove(newTag);
                bool merge = MergeTag(tag, newTag);
                if (merge)
                {
                    updateCount++;
                }
            }
            foreach (Tag tag in newTagList)//handles the new tag from new profile
            {
                Tag oldTag = newProfile.FindTag(tag.TagName);
                if (oldTag != null)
                {
                    continue;
                    //already exist in currentProfile.
                }
                currentProfile.AddTag(tag);
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
            {
                //Since time different , merge.
                if (newTag.IsDeleted && !current.IsDeleted)
                {
                    //Tag is Deleted.
                    SystemLogicLayer.Instance.DeleteTag(newTag.TagName);
                }
                foreach (TaggedPath path in current.PathList)
                {
                    if (!newTag.Contains(path.Path))
                    {
                        //Deleted Path
                        //SystemLogicLayer.Instance.RemoveTagPath(current, path);
                        //current.RemovePath(path.Path, TaggingHelper.GetCurrentTime());
                    }
                }
                foreach (TaggedPath path in newTag.PathList)
                {
                    if (!current.Contains(path.Path))
                    {
                        //New Path
                        SystemLogicLayer.Instance.AddTagPath(current, path);
                        current.AddPath(path);
                    }
                }
            }
            return true;


        }
    }
}
