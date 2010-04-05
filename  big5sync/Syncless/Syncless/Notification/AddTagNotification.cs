using Syncless.Tagging;

namespace Syncless.Notification
{
    public class AddTagNotification : AbstractNotification 
    {
        private Tag _tag;

        public Tag Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }
        public AddTagNotification(Tag tag)
            : base("Add Tag Notification", NotificationCode.ADD_TAG_NOTIFICATION)
        {
            this._tag = tag;
        }
    }
}
