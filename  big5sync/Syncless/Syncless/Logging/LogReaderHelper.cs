using System;
using System.Collections.Generic;
using System.IO;
using Syncless.Helper;

namespace Syncless.Logging
{
    public class LogReaderHelper
    {
        private const string USER_LOG_PATH = @"log\user.log";
        private const int MAX_LOG = 100;

        public static List<LogData> ReadLog()
        {
            List<LogData> logs = new List<LogData>(MAX_LOG);
            StreamReader streamReader;
            try
            {
                streamReader = new StreamReader(USER_LOG_PATH);
            }
            catch (FileNotFoundException)
            {
                return logs;
            }
            while (!streamReader.EndOfStream)
            {
                string text = streamReader.ReadLine();
                string[] tokens = text.Split(new char[] { '~' });
                if (tokens.Length != 4)
                {
                    streamReader.Close();
                    throw new LogFileCorruptedException(ErrorMessage.LOG_FILE_CORRUPTED);
                }
                string timestamp = tokens[0].Trim();
                LogEventType logEvent = Convert(tokens[2].Trim());
                string message = tokens[3].Trim();
                if (logs.Count > MAX_LOG)
                {
                    logs.RemoveAt(0);
                }
                logs.Add(new LogData(timestamp, logEvent, message));
            }
            streamReader.Close();
            return logs;
        }

        private static LogEventType Convert(string type)
        {
            switch (type)
            {
                case "SYNC_STARTED": return LogEventType.SYNC_STARTED;
                case "SYNC_STOPPED": return LogEventType.SYNC_STOPPED;
                case "APPEVENT_DRIVE_ADDED": return LogEventType.APPEVENT_DRIVE_ADDED;
                case "APPEVENT_DRIVE_RENAMED": return LogEventType.APPEVENT_DRIVE_RENAMED;
                case "APPEVENT_PROFILE_LOAD_FAILED": return LogEventType.APPEVENT_PROFILE_LOAD_FAILED;
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
