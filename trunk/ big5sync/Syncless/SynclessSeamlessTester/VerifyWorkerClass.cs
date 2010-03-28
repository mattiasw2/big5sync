using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace SynclessSeamlessTester
{
    public class VerifyWorkerClass
    {
        private List<LazyFileCompare> _fileResults;
        private List<LazyFolderCompare> _folderResults;
        private TestInfo _testInfo;
        private BackgroundWorker _bgWorker;

        public VerifyWorkerClass(List<string> dest, TestInfo testInfo, BackgroundWorker bgWorker)
        {
            _testInfo = testInfo;
            _bgWorker = bgWorker;
            LazyComparer(dest);
        }

        private void LazyComparer(List<string> dest)
        {
            List<LazyFileCompare> fileResults = LazyFileVerifier(dest);
            List<LazyFolderCompare> folderResults = LazyFolderVerifier(dest);
            _testInfo.Compared = true;
            _testInfo.Passed = fileResults.Count == 0 && folderResults.Count == 0;
            _testInfo.FileResults = fileResults;
            _testInfo.FolderResults = folderResults;
        }

        private List<LazyFileCompare> LazyFileVerifier(List<string> dest)
        {
            if (dest.Count == 1)
                return GetLazyFileList(dest[0]);

            LazyFileComparer lazyComparer = new LazyFileComparer();

            var unionFiles = GetLazyFileList(dest[0]).Union(GetLazyFileList(dest[1]), lazyComparer);
            var intersectFiles = GetLazyFileList(dest[0]).Intersect(GetLazyFileList(dest[1]), lazyComparer);

            if (dest.Count == 2)
                return (unionFiles.Except(intersectFiles, lazyComparer)).ToList();

            for (int i = 2; i < dest.Count; i++)
                unionFiles = unionFiles.Union(GetLazyFileList(dest[i]), lazyComparer);

            for (int i = 2; i < dest.Count; i++)
                intersectFiles = intersectFiles.Intersect(GetLazyFileList(dest[i]), lazyComparer);

            return (unionFiles.Except(intersectFiles, lazyComparer)).ToList();
        }

        private List<LazyFolderCompare> LazyFolderVerifier(List<string> dest)
        {
            if (dest.Count == 1)
                return GetLazyFolderList(dest[0]);

            LazyFolderComparer lazyComparer = new LazyFolderComparer();

            var unionFolders = GetLazyFolderList(dest[0]).Union(GetLazyFolderList(dest[1]), lazyComparer);
            var intersectFolders = GetLazyFolderList(dest[0]).Intersect(GetLazyFolderList(dest[1]), lazyComparer);

            if (dest.Count == 2)
                return (unionFolders.Except(intersectFolders, lazyComparer)).ToList();

            for (int i = 2; i < dest.Count; i++)
                unionFolders = unionFolders.Union(GetLazyFolderList(dest[i]), lazyComparer);

            for (int i = 2; i < dest.Count; i++)
                intersectFolders = intersectFolders.Intersect(GetLazyFolderList(dest[i]), lazyComparer);

            return (unionFolders.Except(intersectFolders, lazyComparer)).ToList();
        }

        private List<LazyFileCompare> GetLazyFileList(string path)
        {
            List<LazyFileCompare> results = new List<LazyFileCompare>();
            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            foreach (string f in files)
                if (!f.Contains(".syncless") && !f.Contains("_synclessArchive"))
                    results.Add(new LazyFileCompare(CalculateMD5Hash(f), GetRelativePath(path, f), f));

            return results;
        }

        private List<LazyFolderCompare> GetLazyFolderList(string path)
        {
            List<LazyFolderCompare> results = new List<LazyFolderCompare>();
            string[] folders = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

            foreach (string f in folders)
                if (!f.Contains(".syncless") && !f.Contains("_synclessArchive"))
                    results.Add(new LazyFolderCompare(GetRelativePath(path, f), f));

            return results;
        }


        private string GetRelativePath(string path, string file)
        {
            string result = file.Substring(path.Length);
            result = result.TrimEnd(' ', '\\');
            result = result.TrimStart(' ', '\\');
            return result;
        }

        public static string CalculateMD5Hash(string fullPath)
        {
            try
            {
                FileStream fileStream = new FileInfo(fullPath).OpenRead();
                byte[] fileHash = MD5.Create().ComputeHash(fileStream);
                fileStream.Close();
                return BitConverter.ToString(fileHash).Replace("-", "");
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
            }
            return string.Empty;
        }

        private class LazyFileComparer : IEqualityComparer<LazyFileCompare>
        {
            public bool Equals(LazyFileCompare f1, LazyFileCompare f2)
            {
                return (f1.RelativeName == f2.RelativeName && f1.Hash == f2.Hash);
            }

            public int GetHashCode(LazyFileCompare fi)
            {
                return fi.RelativeName.ToLower().GetHashCode();
            }
        }

        private class LazyFolderComparer : IEqualityComparer<LazyFolderCompare>
        {
            public bool Equals(LazyFolderCompare f1, LazyFolderCompare f2)
            {
                return (f1.RelativeName == f2.RelativeName);
            }

            public int GetHashCode(LazyFolderCompare fi)
            {
                return fi.RelativeName.ToLower().GetHashCode();
            }
        }
    }
}
