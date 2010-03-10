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
    }
}
