using System;
using System.Text;
using System.Diagnostics;

namespace Syncless.Logging
{
    public class LogData
    {
        private string timestamp;
        public string Timestamp
        {
            get
            {
                return timestamp;
            }
            set
            {
                timestamp = value;
            }
        }

        private LogCategoryType logCategory;
        public LogCategoryType LogCategory
        {
            get
            {
                return logCategory;
            }
            set
            {
                logCategory = value;
            }
        }

        private LogEventType logEvent;
        public LogEventType LogEvent
        {
            get
            {
                return logEvent;
            }
            set
            {
                logEvent = value;
            }
        }

        private string message;
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
            }
        }

        public LogData(LogEventType logEvent, string message)
        {
            this.logEvent = logEvent;
            this.message = message;
            switch (logEvent)
            {
                case LogEventType.SYNC_STARTED:
                case LogEventType.SYNC_STOPPED:
                    this.logCategory = LogCategoryType.SYNC;
                    break;
                case LogEventType.APPEVENT_DRIVE_ADDED:
                case LogEventType.APPEVENT_DRIVE_RENAMED:
                case LogEventType.APPEVENT_TAG_CREATED:
                case LogEventType.APPEVENT_TAG_DELETED:
                case LogEventType.APPEVENT_TAG_CONFIG_UPDATED:
                case LogEventType.APPEVENT_FOLDER_TAGGED:
                case LogEventType.APPEVENT_FOLDER_UNTAGGED:
                    this.logCategory = LogCategoryType.APPEVENT;
                    break;
                case LogEventType.FSCHANGE_CREATED:
                case LogEventType.FSCHANGE_MODIFIED:
                case LogEventType.FSCHANGE_DELETED:
                case LogEventType.FSCHANGE_RENAMED:
                case LogEventType.FSCHANGE_ERROR:
                    this.logCategory = LogCategoryType.FSCHANGE;
                    break;
                default:
                    this.logCategory = LogCategoryType.UNKNOWN;
                    break;
            }
        }

        public LogData(string timestamp, LogEventType logEvent, string message)
        {
            this.timestamp = timestamp;
            this.logEvent = logEvent;
            this.message = message;
            switch (logEvent)
            {
                case LogEventType.SYNC_STARTED:
                case LogEventType.SYNC_STOPPED:
                    this.logCategory = LogCategoryType.SYNC;
                    break;
                case LogEventType.APPEVENT_DRIVE_ADDED:
                case LogEventType.APPEVENT_DRIVE_RENAMED:
                case LogEventType.APPEVENT_TAG_CREATED:
                case LogEventType.APPEVENT_TAG_DELETED:
                case LogEventType.APPEVENT_TAG_CONFIG_UPDATED:
                case LogEventType.APPEVENT_FOLDER_TAGGED:
                case LogEventType.APPEVENT_FOLDER_UNTAGGED:
                    this.logCategory = LogCategoryType.APPEVENT;
                    break;
                case LogEventType.FSCHANGE_CREATED:
                case LogEventType.FSCHANGE_MODIFIED:
                case LogEventType.FSCHANGE_DELETED:
                case LogEventType.FSCHANGE_RENAMED:
                case LogEventType.FSCHANGE_ERROR:
                    this.logCategory = LogCategoryType.FSCHANGE;
                    break;
                default:
                    this.logCategory = LogCategoryType.UNKNOWN;
                    break;
            }
        }
    }
}
