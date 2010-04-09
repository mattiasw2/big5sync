namespace Syncless.Notification
{
    public class AutoSyncCompleteNotification : AbstractNotification
    {
        public string Path
        {
            get;
            set;
        }
        public AutoSyncCompleteNotification(string path)
            : base("Auto Sync Complete Notification", Notification.NotificationCode.AutoSyncCompleteNotification)
        {
            Path = path;
        }
    }
}
