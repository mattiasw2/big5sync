using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Syncless.CompareAndSync
{
    public class Syncer
    {
        public List<SyncResult> Sync(List<CompareResult> results)
        {
            foreach (CompareResult result in results)
            {
                switch (result.ChangeType)
                {
                    case FileChangeType.Create:
                        CopyFile(result.From, result.To);
                        break;
                    case FileChangeType.Delete:
                        break;
                    case FileChangeType.Rename:
                        break;
                    case FileChangeType.Update:
                        CopyFile(result.From, result.To);
                        break;
                }
            }

            return null;
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
