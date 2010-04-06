using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Xml;

namespace SynclessSeamlessTester
{
    public class VerifyWorkerClass
    {
        public const string MetaDir = ".syncless";
        public const string XMLName = "syncless.xml";
        public const string LastKnownStateName = "lastknownstate.xml";
        public const string MetadataPath = MetaDir + "\\" + XMLName;
        public const string LastKnownStatePath = MetaDir + "\\" + LastKnownStateName;
        public const string NodeName = "name";
        public const string NodeHash = "hash";
        public const string XPathFolder = "/folder";
        public const string XPathFile = "/file";
        public const string XPathExpr = "/meta-data";
        public const string XPathName = "/" + NodeName;
        public const string XPathHash = "/" + NodeHash;

        private TestInfo _testInfo;
        private BackgroundWorker _bgWorker;
        private DoWorkEventArgs _e;
        private List<string> _filters;
        private TesterState _testState;

        public enum TesterState
        {
            VerifyFiles,
            VerifyMeta,
            VerifyTodo
        }

        public VerifyWorkerClass(List<string> dest, TestInfo testInfo, BackgroundWorker bgWorker, DoWorkEventArgs e, List<string> filters, TesterState testerState)
        {
            _testInfo = testInfo;
            _bgWorker = bgWorker;
            _e = e;
            _filters = filters;
            _testState = testerState;
            LazyComparer(dest);
        }

        private void LazyComparer(List<string> dest)
        {
            List<LazyFileCompare> fileResults = null;
            List<LazyFolderCompare> folderResults = null;

            switch (_testState)
            {
                case TesterState.VerifyFiles:
                    fileResults = LazyFileVerifier(dest);
                    folderResults = LazyFolderVerifier(dest);
                    break;
                case TesterState.VerifyMeta:
                    fileResults = LazyMetaFileVerifier(dest);
                    folderResults = LazyMetaFolderVerifier(dest);
                    break;
            }

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
            {
                if (_bgWorker.CancellationPending)
                    _e.Cancel = true;
                else
                    unionFiles = unionFiles.Union(GetLazyFileList(dest[i]), lazyComparer);
            }

            for (int i = 2; i < dest.Count; i++)
            {
                if (_bgWorker.CancellationPending)
                    _e.Cancel = true;
                else
                    intersectFiles = intersectFiles.Intersect(GetLazyFileList(dest[i]), lazyComparer);
            }

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
            {
                if (_bgWorker.CancellationPending)
                    _e.Cancel = true;
                else
                    unionFolders = unionFolders.Union(GetLazyFolderList(dest[i]), lazyComparer);
            }

            for (int i = 2; i < dest.Count; i++)
            {
                if (_bgWorker.CancellationPending)
                    _e.Cancel = true;
                else
                    intersectFolders = intersectFolders.Intersect(GetLazyFolderList(dest[i]), lazyComparer);
            }

            return (unionFolders.Except(intersectFolders, lazyComparer)).ToList();
        }

        private List<LazyFileCompare> LazyMetaFileVerifier(List<string> dest)
        {
            if (dest.Count == 1)
                return GetLazyMetaFileList(dest[0]);

            LazyFileComparer lazyComparer = new LazyFileComparer();

            var unionFiles = GetLazyMetaFileList(dest[0]).Union(GetLazyMetaFileList(dest[1]), lazyComparer);
            var intersectFiles = GetLazyMetaFileList(dest[0]).Intersect(GetLazyMetaFileList(dest[1]), lazyComparer);

            if (dest.Count == 2)
                return (unionFiles.Except(intersectFiles, lazyComparer)).ToList();

            for (int i = 2; i < dest.Count; i++)
            {
                if (_bgWorker.CancellationPending)
                    _e.Cancel = true;
                else
                    unionFiles = unionFiles.Union(GetLazyMetaFileList(dest[i]), lazyComparer);
            }

            for (int i = 2; i < dest.Count; i++)
            {
                if (_bgWorker.CancellationPending)
                    _e.Cancel = true;
                else
                    intersectFiles = intersectFiles.Intersect(GetLazyMetaFileList(dest[i]), lazyComparer);
            }

            return (unionFiles.Except(intersectFiles, lazyComparer)).ToList();
        }

        private List<LazyFolderCompare> LazyMetaFolderVerifier(List<string> dest)
        {
            if (dest.Count == 1)
                return GetLazyMetaFolderList(dest[0]);

            LazyFolderComparer lazyComparer = new LazyFolderComparer();

            var unionFolders = GetLazyMetaFolderList(dest[0]).Union(GetLazyMetaFolderList(dest[1]), lazyComparer);
            var intersectFolders = GetLazyMetaFolderList(dest[0]).Intersect(GetLazyMetaFolderList(dest[1]), lazyComparer);

            if (dest.Count == 2)
                return (unionFolders.Except(intersectFolders, lazyComparer)).ToList();

            for (int i = 2; i < dest.Count; i++)
            {
                if (_bgWorker.CancellationPending)
                    _e.Cancel = true;
                else
                    unionFolders = unionFolders.Union(GetLazyMetaFolderList(dest[i]), lazyComparer);
            }

            for (int i = 2; i < dest.Count; i++)
            {
                if (_bgWorker.CancellationPending)
                    _e.Cancel = true;
                else
                    intersectFolders = intersectFolders.Intersect(GetLazyMetaFolderList(dest[i]), lazyComparer);
            }

            return (unionFolders.Except(intersectFolders, lazyComparer)).ToList();
        }

        private List<LazyFileCompare> GetLazyMetaFileList(string path)
        {
            List<LazyFileCompare> results = new List<LazyFileCompare>();
            string[] folders = Directory.GetDirectories(path, ".syncless", SearchOption.AllDirectories);

            foreach (string f in folders)
            {
                string metaPath = Path.Combine(f, XMLName);

                if (File.Exists(metaPath))
                {
                    string actualPath = Directory.GetParent(f).FullName;
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(metaPath);
                    XmlNodeList nodeList = xmlDoc.SelectNodes(XPathExpr + XPathFile);
                    string relativeName = string.Empty, fullPath = string.Empty, hash = string.Empty;

                    foreach (XmlNode node in nodeList)
                    {
                        foreach (XmlNode innerStupidNode in node.ChildNodes)
                        {
                            switch (innerStupidNode.Name)
                            {
                                case NodeName:
                                    fullPath = Path.Combine(actualPath, innerStupidNode.InnerText);
                                    relativeName = GetRelativePath(path, fullPath);
                                    break;
                                case NodeHash:
                                    hash = innerStupidNode.InnerText;
                                    break;
                            }
                        }
                        results.Add(new LazyFileCompare(hash, relativeName, fullPath));
                    }

                }
            }

            return results;
        }

        private List<LazyFolderCompare> GetLazyMetaFolderList(string path)
        {
            List<LazyFolderCompare> results = new List<LazyFolderCompare>();
            string[] folders = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

            foreach (string f in folders)
            {
                string metaPath = Path.Combine(f, XMLName);

                if (File.Exists(metaPath))
                {
                    string actualPath = Directory.GetParent(f).FullName;
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(metaPath);
                    XmlNodeList nodeList = xmlDoc.SelectNodes(XPathExpr + XPathFolder);
                    string relativeName = string.Empty, fullPath = string.Empty;

                    foreach (XmlNode node in nodeList)
                    {
                        foreach (XmlNode innerStupidNode in node.ChildNodes)
                        {
                            switch (innerStupidNode.Name)
                            {
                                case NodeName:
                                    fullPath = Path.Combine(actualPath, innerStupidNode.InnerText);
                                    relativeName = GetRelativePath(path, fullPath);
                                    break;
                            }
                        }
                        results.Add(new LazyFolderCompare(relativeName, fullPath));
                    }

                }
            }

            return results;
        }

        private List<LazyFileCompare> GetLazyFileList(string path)
        {
            List<LazyFileCompare> results = new List<LazyFileCompare>();
            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            foreach (string f in files)
                if (PassFilter(f))
                    results.Add(new LazyFileCompare(CalculateMD5Hash(f), GetRelativePath(path, f), f));

            return results;
        }

        private List<LazyFolderCompare> GetLazyFolderList(string path)
        {
            List<LazyFolderCompare> results = new List<LazyFolderCompare>();
            string[] folders = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

            foreach (string f in folders)
                if (PassFilter(f))
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

        private bool PassFilter(string s)
        {
            foreach (string f in _filters)
                if (s.Contains(f))
                    return false;
            return true;
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
