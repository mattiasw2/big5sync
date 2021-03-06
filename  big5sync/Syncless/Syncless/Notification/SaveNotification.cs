﻿/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
namespace Syncless.Notification
{
    /// <summary>
    /// The Nofication for saving all the data.
    /// </summary>
    public class SaveNotification : AbstractNotification
    {
        /// <summary>
        /// Initialize SaveNotification
        /// </summary>
        public SaveNotification()
            : base("Save Notification", NotificationCode.SaveNotification)
        {

        }
        /// <summary>
        /// Override the Equal method
        /// </summary>
        /// <param name="obj">object to compare</param>
        /// <returns>true if it is Equals, false otherwise</returns>
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return true; // return true as long as their type is the same.
            

        }

        /// <summary>
        /// override object.GetHashCode
        /// </summary>
        /// <returns>the int of the HashCode of this object</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
