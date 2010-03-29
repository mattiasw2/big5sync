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
                logCategory = MapEventToCategory(logEvent);
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
            this.logCategory = MapEventToCategory(logEvent);
        }

        public LogData(string timestamp, LogEventType logEvent, string message)
        {
            this.timestamp = timestamp;
            this.logEvent = logEvent;
            this.message = message;
            this.logCategory = MapEventToCategory(logEvent);
        }

        private LogCategoryType MapEventToCategory(LogEventType type)
        {
            switch (type)
            {
                case LogEventType.SYNC_STARTED:
                case LogEventType.SYNC_STOPPED: return LogCategoryType.SYNC;
                case LogEventType.APPEVENT_DRIVE_ADDED:
                case LogEventType.APPEVENT_DRIVE_RENAMED:
                case LogEventType.APPEVENT_TAG_CREATED:
                case LogEventType.APPEVENT_TAG_DELETED:
                case LogEventType.APPEVENT_TAG_CONFIG_UPDATED:
                case LogEventType.APPEVENT_FOLDER_TAGGED:
                case LogEventType.APPEVENT_FOLDER_UNTAGGED: return LogCategoryType.APPEVENT;
                case LogEventType.FSCHANGE_CREATED:
                case LogEventType.FSCHANGE_MODIFIED:
                case LogEventType.FSCHANGE_DELETED:
                case LogEventType.FSCHANGE_RENAMED:
                case LogEventType.FSCHANGE_ERROR: return LogCategoryType.FSCHANGE;
                default: return LogCategoryType.UNKNOWN;
            }
        }
    }
}
