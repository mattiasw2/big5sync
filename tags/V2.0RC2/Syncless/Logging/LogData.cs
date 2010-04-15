/*
 * 
 * Author: Koh Cher Guan
 * 
 */

namespace Syncless.Logging
{
    /// <summary>
    /// A Data Transfer Object to pass log information
    /// </summary>
    public class LogData
    {
        private string timestamp;
        /// <summary>
        /// Gets or sets a value for the timestamp.
        /// </summary>
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
        /// <summary>
        /// Gets a value for the log category type.
        /// </summary>
        public LogCategoryType LogCategory
        {
            get
            {
                return logCategory;
            }
        }

        private LogEventType logEvent;
        /// <summary>
        /// Gets or sets a value for the log event type.
        /// </summary>
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
        /// <summary>
        /// Gets or sets a value for the log message.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Logging.LogData" /> class, given the log event type and the message.
        /// </summary>
        /// <param name="logEvent">A <see cref="System.IO.DirectoryInfo" /> class specifying the log event type.</param>
        /// <param name="message">A <see cref="string" /> specifying the message.</param>
        public LogData(LogEventType logEvent, string message)
        {
            this.logEvent = logEvent;
            this.message = message;
            this.logCategory = MapEventToCategory(logEvent);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Logging.LogData" /> class, given the time stamp, the log event type and the message.
        /// </summary>
        /// <param name="timestamp">A <see cref="string" /> specifying the time stamp.</param>
        /// <param name="logEvent">A <see cref="System.IO.DirectoryInfo" /> class specifying the log event type.</param>
        /// <param name="message">A <see cref="string" /> specifying the message.</param>
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
                case LogEventType.FSCHANGE_ARCHIVED:
                case LogEventType.FSCHANGE_CONFLICT:
                case LogEventType.FSCHANGE_ERROR: return LogCategoryType.FSCHANGE;
                default: return LogCategoryType.UNKNOWN;
            }
        }
    }
}
