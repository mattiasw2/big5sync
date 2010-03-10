using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync
{
    public class SeamlessSyncer
    {
        public static void Sync(string sourceName, string sourceParent, List<string> destinations, bool? isFolder, AutoSyncRequestType? requestType)
        {
            bool isFldr;

            if (isFolder.HasValue)
                isFldr = (bool)isFolder;
            else
                isFldr = IsFolder(sourceName, sourceParent, destinations);

            if (isFldr)
                SyncFolder(sourceName, sourceParent, destinations, requestType);
            else
                SyncFile(sourceName, sourceParent, destinations, requestType);
        }

        private static void SyncFile(string sourceName, string sourceParent, List<string> destinations, AutoSyncRequestType? requestType)
        {

        }

        private static void SyncFolder(string sourceName, string sourceParent, List<string> destinations, AutoSyncRequestType? requestType)
        {

        }

        private static bool IsFolder(string sourceName, string sourceParent, List<string> destinations)
        {

            bool result = false;
            return result;
        }
    }
}
