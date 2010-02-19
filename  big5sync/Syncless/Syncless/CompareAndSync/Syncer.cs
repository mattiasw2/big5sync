using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Syncless.CompareAndSync
{
    public class Syncer
    {
        public List<SyncResult> SyncFile(List<string> paths, List<CompareResult> results)
        {
            return null;
        }

        public List<SyncResult> SyncFolder(List<string> paths, List<CompareResult> results)
        {
            List<SyncResult> syncResults = new List<SyncResult>();
            foreach (CompareResult result in results)
            {
                switch (result.ChangeType)
                {
                    case FileChangeType.Create:
                        syncResults.Add(CreateFile(result.From, result.To));
                        break;
                    case FileChangeType.Delete:
                        syncResults.Add(DeleteFile(result.From));
                        break;
                    case FileChangeType.Rename:
                        syncResults.Add(MoveFile(result.From, result.To));
                        break;
                    case FileChangeType.Update:
                        syncResults.Add(UpdateFile(result.From, result.To));
                        break;
                }
            }

            //TODO: Will handle each change separately in future
            foreach (string path in paths)
            {
                XMLHelper.GenerateXMLFile(path);
            }

            return syncResults;
        }

        public SyncResult DeleteFile(string from)
        {
            try
            {
                File.Delete(from);
                return new SyncResult(FileChangeType.Delete, from, true);
            }
            catch (Exception)
            {
                return new SyncResult(FileChangeType.Delete, from, false);
            }
        }

        public SyncResult MoveFile(string from, string to)
        {
            FileInfo source = new FileInfo(from);
            FileInfo target = new FileInfo(to);

            try
            {
                if (!target.Exists)
                {
                    if (!Directory.Exists(target.Directory.FullName))
                    {
                        Directory.CreateDirectory(target.Directory.FullName);
                    }
                }
                File.Move(from, to);
                return new SyncResult(FileChangeType.Rename, from, to, true);
            }
            catch (Exception)
            {
                return new SyncResult(FileChangeType.Rename, from, to, false);
            }
        }

        private SyncResult CopyFile(string from, string to, FileChangeType changeType)
        {
            FileInfo source = new FileInfo(from);
            FileInfo target = new FileInfo(to);
            
            if (!target.Exists)
            {
                if (!Directory.Exists(target.Directory.FullName))
                {
                    Directory.CreateDirectory(target.Directory.FullName);
                }               
            }
            
            try {
                source.CopyTo(to, true);
                return new SyncResult(changeType, from, to, true);
            }
            catch (Exception)
            {
                return new SyncResult(changeType, from, to, false);
            }
        }

        public SyncResult CreateFile(string from, string to)
        {
            return CopyFile(from, to, FileChangeType.Create);
        }

        public SyncResult UpdateFile(string from, string to)
        {
            return CopyFile(from, to, FileChangeType.Update);
        }
    }
}
