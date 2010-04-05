namespace Syncless.Notification
{
    public class MonitorTagNotification :AbstractNotification
    {
        private string _tagname;

        public string Tagname
        {
            get { return _tagname; }
            set { _tagname = value; }
        }

        public MonitorTagNotification(string tagname)
            : base("Monitor Tag Notification", Syncless.Notification.NotificationCode.MONITOR_TAG_NOTIFICATION)
        {
            _tagname = tagname;
        }
    }
}
