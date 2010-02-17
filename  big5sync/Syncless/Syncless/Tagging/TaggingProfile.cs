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

        private List<FolderTag> _folderTagList;

        public List<FolderTag> FolderTagList
        {
            get { return _folderTagList; }
            set { _folderTagList = value; }
        }

        private List<FileTag> _fileTagList;

        public List<FileTag> FileTagList
        {
            get { return _fileTagList; }
            set { _fileTagList = value; }
        }

        public TaggingProfile()
        {
            _folderTagList = new List<FolderTag>();
            _fileTagList = new List<FileTag>();
        }
    }
}
