using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Notification
{
    public static class NotificationCode
    {
        #region for User Interface
        public const string SYNC_START_NOTIFICATION = "001";
        public const string SYNC_COMPLETE_NOTIFICATION = "002";
        public const string COMPARE_COMPLETE_NOTIFICATION = "003";
        public const string NEW_PROFILE_NOTIFICATION = "201";
        public const string NOTHING_TO_SYNC_NOTIFICATION = "202";
        public const string Tagged_Folder_Deleted_Notification = "203";
        #endregion


        #region for System Logic Layer
        public const string MONITOR_PATH_NOTIFICATION = "100";
        public const string UNMONITOR_PATH_NOTIFICATION = "101";

        public const string ADD_TAG_NOTIFICATION = "110";
        public const string DEL_TAG_NOTIFICATION = "111";

        public const string MONITOR_TAG_NOTIFICATION = "121";

        public const string TAGGED_PATH_DELETED_NOTIFICATION = "130";
        #endregion
        
    }
}
