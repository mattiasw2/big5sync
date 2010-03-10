using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync
{
    public class SeamlessSyncer
    {
        public static void Sync(string source, List<string> destinations, bool? isFolder, AutoSyncRequestType? requestType)
        {
            if (isFolder.HasValue)
            {
                if ((bool)isFolder)
                    SyncFolder(source, destinations, requestType);
                else
                    SyncFile(source, destinations, requestType);
            }
            else
            {
                if (IsFolder(source, destinations))
                {
                }
                else
                {
                }
            }

        }

        private static void SyncFile(string source, List<string> destinations, AutoSyncRequestType? requestType)
        {

        }

        private static void SyncFolder(string source, List<string> destinations, AutoSyncRequestType? requestType)
        {

        }

        private static bool IsFolder(string source, List<string> destinations)
        {
            bool result = false;
            return result;
        }
    }
}
