/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
using Syncless.Tagging;

namespace Syncless.Notification
{
    /// <summary>
    /// The notification to inform SLL to monitor a tag.
    /// </summary>
    public class MonitorTagNotification :AbstractNotification
    {
        /// <summary>
        /// Get and Set the name of the <see cref="Tag"/>
        /// </summary>
        public string Tagname { get; set; }
        /// <summary>
        /// Initialize the Monitor Tag Noficiation
        /// </summary>
        /// <param name="tagname"></param>
        public MonitorTagNotification(string tagname)
            : base("Monitor Tag Notification", NotificationCode.MonitorTagNotification)
        {
            Tagname = tagname;
        }
    }
}
