using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging
{
    public class TaggingProfile
    {
        private string _profileName;

        public string ProfileName
        {
            get { return _profileName; }
            set { _profileName = value; }
        }

        private long _lastUpdated;

        public long LastUpdated
        {
            get { return _lastUpdated; }
            set { _lastUpdated = value; }
        }

        private long _created;

        public long Created
        {
            get { return _created; }
            set { _created = value; }
        }

        private List<Tag> _tagList;

        public List<Tag> TagList
        {
            get { return _tagList; }
            set { _tagList = value; }
        }

        public TaggingProfile(long created)
        {
            _tagList = new List<Tag>();
            _created = created;
            _lastUpdated = created;
        }

        public void AddTag(Tag tag)
        {
            if (!Contains(tag.TagName))
            {
                _tagList.Add(tag);
                _lastUpdated = TaggingHelper.GetCurrentTime();
            }
        }

        public bool DeleteTag(Tag tag)
        {
            CurrentTime updated = new CurrentTime();
            Tag toRemove = FindTag(tag.TagName);
            if (toRemove != null)
            {
                if (toRemove.IsDeleted)
                {
                    return false;
                }
                else
                {
                    toRemove.IsDeleted = true;
                    toRemove.DeletedDate = updated.CurrentTimeLong;
                    toRemove.LastUpdated = updated.CurrentTimeLong;
                    toRemove.RemoveAllPaths();
                    _lastUpdated = updated.CurrentTimeLong;
                    return true;
                }
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

        public bool Contains(string tagname)
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
