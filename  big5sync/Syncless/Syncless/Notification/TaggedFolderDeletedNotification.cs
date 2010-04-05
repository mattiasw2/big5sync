using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    public class TaggedFolderDeletedNotification : AbstractNotification
    {
        private string _path;
        private string _tagName;

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public string TagName
        {
            get { return _tagName; }
            set { _tagName = value; }
        }

        public TaggedFolderDeletedNotification(string path,string tagName)
            : base("Tagged Folder Delete Notification", Syncless.Notification.NotificationCode.Tagged_Folder_Deleted_Notification)
        {
            _path = path;
            _tagName = tagName;
        }
    }
}
