using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.Tagging;

namespace Syncless.Notification
{
    /// <summary>
    /// NotificationFactory class provides abstraction to create instances of classes which extend the 
    /// <see cref="AbstractNotification">AbstractNotification</see> class.
    /// </summary>
    public static class NotificationFactory
    {
        /// <summary>
        /// Creates a tag added notification
        /// </summary>
        /// <param name="tag">The <see cref="Tag">Tag</see> object that represents the tag that is added</param>
        /// <returns>the <see cref="AddTagNotification">AddTagNotification</see> object</returns>
        public static AbstractNotification CreateAddTagNotification(Tag tag)
        {
            return new AddTagNotification(tag);
        }

        /// <summary>
        /// Creates a auto sync completed notification
        /// </summary>
        /// <param name="path">The string value that represents the path that is synced</param>
        /// <returns>the <see cref="AutoSyncCompleteNotification">AutoSyncCompleteNotification</see> 
        /// object</returns>
        public static AbstractNotification CreateAutoSyncCompleteNotification(string path)
        {
            return new AutoSyncCompleteNotification(path);
        }

        /// <summary>
        /// Creates a message notification
        /// </summary>
        /// <param name="message">The string value that represents the message</param>
        /// <returns>the <see cref="MessageNotification">MessageNotification</see> object</returns>
        public static AbstractNotification CreateMessageNotification(string message)
        {
            return new MessageNotification(message);
        }

        /// <summary>
        /// Creates a monitor path notification
        /// </summary>
        /// <param name="tag">The <see cref="Tag">Tag</see> object that represents the tag</param>
        /// <param name="path">The string value that represents the path that is monitored</param>
        /// <returns>the <see cref="MonitorPathNotification">MonitorPathNotification</see> object</returns>
        public static AbstractNotification CreateMonitorPathNotification(Tag tag, TaggedPath path)
        {
            return new MonitorPathNotification(tag, path);
        }

        /// <summary>
        /// Creates a monitor tag notification
        /// </summary>
        /// <param name="tagName">The string value that represents the tag name of the tag to be
        /// monitored</param>
        /// <returns>the <see cref="MonitorTagNotification">MonitorTagNotification</see> object</returns>
        public static AbstractNotification CreateMonitorTagNotification(string tagName)
        {
            return new MonitorTagNotification(tagName);
        }

        /// <summary>
        /// Creates a nothing to sync notification
        /// </summary>
        /// <param name="tagName">The string value that represents the tag name</param>
        /// <returns>the <see cref="NothingToSyncNotification">NothingToSyncNotification</see> object</returns>
        public static AbstractNotification CreateNothingToSyncNotification(string tagName)
        {
            return new NothingToSyncNotification(tagName);
        }

        /// <summary>
        /// Creates a tag removed notification
        /// </summary>
        /// <param name="tag">The <see cref="Tag">Tag</see> object that represents the tag that is 
        /// removed</param>
        /// <returns>the <see cref="RemoveTagNotification">RemoveTagNotification</see> object</returns>
        public static AbstractNotification CreateRemoveTagNotification(Tag tag)
        {
            return new RemoveTagNotification(tag);
        }

        /// <summary>
        /// Creates a sync completed notification
        /// </summary>
        /// <param name="tagName">The string value that represents the tag name the sync belongs to</param>
        /// <param name="rco">The <see cref="RootCompareObject">RootCompareObject</see> object that is used
        /// for sync</param>
        /// <returns>the <see cref="SyncCompleteNotification">SyncCompleteNotification</see> 
        /// object</returns>
        public static AbstractNotification CreateSyncCompleteNotification(string tagName, RootCompareObject rco)
        {
            return new SyncCompleteNotification(tagName, rco);
        }

        /// <summary>
        /// Creates a tagged folders deleted notification
        /// </summary>
        /// <param name="path">The string value that represents the path of the folder that is deleted</param>
        /// <param name="tagName">The string value that represents the tag name the folder is tagged to</param>
        /// <returns>the <see cref="TaggedFolderDeletedNotification">TaggedFolderDeletedNotification</see> 
        /// object</returns>
        public static AbstractNotification CreateTaggedFolderDeletedNotification(string path, string tagName)
        {
            return new TaggedFolderDeletedNotification(path, tagName);
        }

        /// <summary>
        /// Creates a tagged paths deleted notification
        /// </summary>
        /// <param name="deletedPaths">The list of strings that represents the list of deleted tagged
        /// paths</param>
        /// <returns>the <see cref="TaggedPathDeletedNotification">TaggedPathDeletedNotification</see> 
        /// object</returns>
        public static AbstractNotification CreateTaggedPathDeletedNotification(List<string> deletedPaths)
        {
            return new TaggedPathDeletedNotification(deletedPaths);
        }

        /// <summary>
        /// Creates a unmonitor path notification
        /// </summary>
        /// <param name="tag">The <see cref="Tag">Tag</see> object that represents the tag</param>
        /// <param name="path"></param>
        /// <returns>the <see cref="UnMonitorPathNotification">UnMonitorPathNotification</see> object</returns>
        public static AbstractNotification CreateUnMonitorPathNotification(Tag tag, TaggedPath path)
        {
            return new UnMonitorPathNotification(tag, path);
        }
    }
}
