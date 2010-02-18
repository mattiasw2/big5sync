using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Syncless.CompareAndSync
{
    public class Syncer
    {
        public List<SyncResult> SyncFile(string tagName, List<string> paths, List<CompareResult> results)
        {
            return null;
        }

        public List<SyncResult> SyncFolder(string tagName, List<string> paths, List<CompareResult> results)
        {
            foreach (CompareResult result in results)
            {
                switch (result.ChangeType)
                {
                    case FileChangeType.Create:
                        CopyFile(result.From, result.To);
                        break;
                    case FileChangeType.Delete:
                        DeleteFile(result.From);
                        break;
                    case FileChangeType.Rename:
                        MoveFile(result.From, result.To);
                        break;
                    case FileChangeType.Update:
                        CopyFile(result.From, result.To);
                        break;
                }
            }

            //TODO: Will handle each change separately in future
            foreach (string path in paths)
            {
                XMLHelper.GenerateXMLFile(tagName, path);
            }

            return null;
        }

        public bool DeleteFile(string from)
        {
            File.Delete(from);
            return true;
        }

        public bool MoveFile(string from, string to)
        {
            try
            {
                File.Move(from, to);
            }
            catch (Exception)
            {
            }
            return true;
        }

        public bool CopyFile(string from, string to)
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
            if (source.Exists)
            {
                source.CopyTo(to, true);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
