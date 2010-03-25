using System;
using System.Collections.Generic;
using System.IO;
using Syncless.Helper;

namespace Syncless.Logging
{
    public class LogReaderHelper
    {
        private const string USER_LOG_PATH = @"log\user.log";

        public static List<LogData> ReadLog()
        {
            StreamReader streamReader = new StreamReader(USER_LOG_PATH);
            List<LogData> logs = new List<LogData>();
            while (!streamReader.EndOfStream)
            {
                string text = streamReader.ReadLine();
                string[] tokens = text.Split(new char[] { '~' });
                if (tokens.Length != 4)
                {
                    throw new LogFileCorruptedException(ErrorMessage.LOG_FILE_CORRUPTED);
                }
                string timestamp = tokens[0].Trim();
                LogEventType logEvent = Convert(tokens[2].Trim());
                string message = tokens[3].Trim();
                logs.Add(new LogData(timestamp, logEvent, message));
            }
            streamReader.Close();
            return logs;
        }

        private static LogEventType Convert(string type)
        {
            switch (type)
            {
                case "SYNC_START": return LogEventType.SYNC_START;
                case "SYNC_STOP": return LogEventType.SYNC_STOP;
                case "APPEVENT_DRIVE_ADDED": return LogEventType.APPEVENT_DRIVE_ADDED;
                case "APPEVENT_DRIVE_RENAMED": return LogEventType.APPEVENT_DRIVE_RENAMED;
                case "APPEVENT_TAG_CREATED": return LogEventType.APPEVENT_TAG_CREATED;
                case "APPEVENT_TAG_DELETED": return LogEventType.APPEVENT_TAG_DELETED;
                case "APPEVENT_TAG_CONFIG_UPDATED": return LogEventType.APPEVENT_TAG_CONFIG_UPDATED;
                case "APPEVENT_FOLDER_TAGGED": return LogEventType.APPEVENT_FOLDER_TAGGED;
                case "APPEVENT_FOLDER_UNTAGGED": return LogEventType.APPEVENT_FOLDER_UNTAGGED;
                case "FSCHANGE_CREATED": return LogEventType.FSCHANGE_CREATED;
                case "FSCHANGE_MODIFIED": return LogEventType.FSCHANGE_MODIFIED;
                case "FSCHANGE_DELETED": return LogEventType.FSCHANGE_DELETED;
                case "FSCHANGE_RENAMED": return LogEventType.FSCHANGE_RENAMED;
                case "FSCHANGE_ERROR": return LogEventType.FSCHANGE_ERROR;
                default: return LogEventType.UNKNOWN;
            }
        }

    }
}
