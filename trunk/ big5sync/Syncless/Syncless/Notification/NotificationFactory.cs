using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;
using Syncless.Tagging;

namespace Syncless.Notification
{
    public static class NotificationFactory 
    {
        public static AbstractNotification CreateAddTagNotification(Tag tag)
        {
            return new AddTagNotification(tag);
        }
        public static AbstractNotification CreateAutoSyncCompleteNotification(string path)
        {
            return new AutoSyncCompleteNotification(path);
        }
        public static AbstractNotification CreateMessageNotification(string message)
        {
            return new MessageNotification(message);
        }
        public static AbstractNotification CreateMonitorPathNotification(Tag tag , TaggedPath path)
        {
            return new MonitorPathNotification(tag, path);
        }
        public static AbstractNotification CreateMonitorTagNotification(string tagName)
        {
            return new MonitorTagNotification(tagName);
        }
        public static AbstractNotification CreateNothingToSyncNotification(string tagName)
        {
            return new NothingToSyncNotification(tagName);
        }
        public static AbstractNotification CreateRemoveTagNotification(Tag tag)
        {
            return new RemoveTagNotification(tag);
        }
        public static AbstractNotification CreateSyncCompleteNotification(string tagName,RootCompareObject rco)
        {
            return new SyncCompleteNotification(tagName, rco);
        }
        public static AbstractNotification CreateTaggedFolderDeletedNotification(string path , string tagName)
        {
            return new TaggedFolderDeletedNotification(path, tagName);
        }
        public static AbstractNotification CreateTaggedPathDeletedNotification(List<string> deletedPaths)
        {
            return new TaggedPathDeletedNotification(deletedPaths);
        }
        public static AbstractNotification CreateUnMonitorPathNotification(Tag tag,TaggedPath path)
        {
            return new UnMonitorPathNotification(tag, path);
        }
    }
}
