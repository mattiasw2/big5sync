using System;
using System.Collections.Generic;
using System.IO;
using Syncless.Helper;

namespace Syncless.Logging
{
    public class LogReaderHelper
    {
        private const string USER_LOG_PATH = @"log\user.log.1";
        private const string USER_LOG_BACKUP_PATH = @"log\user.log";
        private const int MAX_LOG = 1000;

        public static List<LogData> ReadLog()
        {
            List<LogData> logs = new List<LogData>();
            FileStream fs = null;
            StreamReader streamReader = null;
            try
            {
                fs = new FileStream(USER_LOG_BACKUP_PATH, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                streamReader = new StreamReader(fs);
                ReadFromPath(streamReader, logs);
            }
            catch (FileNotFoundException)
            { }
            catch (LogFileCorruptedException e)
            {
                streamReader.Close();
                fs.Close();
                File.Delete(USER_LOG_BACKUP_PATH);
                throw e;
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
            }
            
            try
            {
                fs = new FileStream(USER_LOG_PATH, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                streamReader = new StreamReader(fs);
                ReadFromPath(streamReader, logs);
            }
            catch (FileNotFoundException)
            { }
            catch (LogFileCorruptedException e)
            {
                streamReader.Close();
                fs.Close();
                File.Delete(USER_LOG_BACKUP_PATH);
                throw e;
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
            }
            return logs;
        }

        private static void ReadFromPath(StreamReader streamReader, List<LogData> logs)
        {
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
                if (logs.Count == MAX_LOG)
                {
                    logs.RemoveAt(0);
                }
                logs.Add(new LogData(timestamp, logEvent, message));
            }
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
                case "FSCHANGE_ARCHIVED": return LogEventType.FSCHANGE_ARCHIVED;
                case "FSCHANGE_ERROR": return LogEventType.FSCHANGE_ERROR;
                default: return LogEventType.UNKNOWN;
            }
        }

    }
}
