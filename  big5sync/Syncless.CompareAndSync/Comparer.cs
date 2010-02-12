using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace Syncless.CompareAndSync
{
    public class Comparer
    {
        private const int CREATE_TABLE = 0, DELETE_TABLE = 1, RENAME_TABLE = 2, UPDATE_TABLE = 3;
        private Dictionary<int, Dictionary<string, List<string>>> _changeTable;
        private Dictionary<string, string> propogateTable;

        public List<CompareResult> CompareFolder(DummyTag dTag)
        {
            Debug.Assert(dTag != null);
            _changeTable = new Dictionary<int, Dictionary<string, List<string>>>();
            _changeTable.Add(CREATE_TABLE, new Dictionary<string, List<string>>());
            _changeTable.Add(DELETE_TABLE, new Dictionary<string, List<string>>());
            _changeTable.Add(RENAME_TABLE, new Dictionary<string, List<string>>());
            _changeTable.Add(UPDATE_TABLE, new Dictionary<string, List<string>>());
            propogateTable = new Dictionary<string, string>();

            List<string> paths = dTag.Paths;
            List<FileInfo> currSrcFolder = GetAllDirectoryFiles(paths[0]);

            for (int i = 1; i < paths.Count; i++)
            {
                currSrcFolder = OneWayCompareFolder(paths[i - 1], paths[i], currSrcFolder, GetAllDirectoryFiles(paths[i]));
            }
            for (int i = paths.Count - 2; i >= 0; i--)
            {
                currSrcFolder = OneWayCompareFolder(paths[i + 1], paths[i], currSrcFolder, GetAllDirectoryFiles(paths[i]));
            }
            ProcessRawResults();
            return null;
        }

        public List<FileInfo> OneWayCompareFolder(string sourcePath, string targetPath, List<FileInfo> source, List<FileInfo> target)
        {
            Debug.Assert(source != null && target != null);
            List<FileInfo> querySrcExceptTgt = source.Except(target, new FileNameCompare()).ToList<FileInfo>();
            List<FileInfo> querySrcIntersectTgt = source.Intersect(target, new FileNameCompare()).ToList<FileInfo>();
            List<FileInfo> queryTgtIntersectSrc = target.Intersect(source, new FileNameCompare()).ToList<FileInfo>();
            int exceptItemsCount = querySrcExceptTgt.Count;
            Debug.Assert(querySrcIntersectTgt.Count == queryTgtIntersectSrc.Count);
            int commonItemsCount = queryTgtIntersectSrc.Count;
            List<string> createList, deleteList, renameList, updateList;
            string newFilePath = null;

            for (int i = 0; i < exceptItemsCount; i++)
            {
                newFilePath = CreateNewItemPath(sourcePath, querySrcExceptTgt[i].FullName, targetPath);
                if (_changeTable[CREATE_TABLE].TryGetValue(querySrcExceptTgt[i].FullName, out createList))
                {
                    if (!createList.Contains(newFilePath))
                    {
                        createList.Add(newFilePath);
                    }
                }
                else
                {
                    createList = new List<string>();
                    createList.Add(newFilePath);
                    _changeTable[CREATE_TABLE].Add(querySrcExceptTgt[i].FullName, createList);
                }

                if (!propogateTable.ContainsKey(querySrcExceptTgt[i].FullName))
                {
                    propogateTable.Add(querySrcExceptTgt[i].FullName, sourcePath);
                }

            }

            for (int i = 0; i < commonItemsCount; i++)
            {
                FileInfo srcFile = (FileInfo)querySrcIntersectTgt[i];
                FileInfo tgtFile = (FileInfo)queryTgtIntersectSrc[i];
                int compareResult = new FileContentCompare().Compare(srcFile, tgtFile);

                if (compareResult > 0)
                {
                    if (_changeTable[UPDATE_TABLE].TryGetValue(srcFile.FullName, out updateList))
                    {
                        if (!updateList.Contains(tgtFile.FullName))
                        {
                            updateList.Add(tgtFile.FullName);
                        }
                    }
                    else
                    {
                        updateList = new List<string>();
                        updateList.Add(tgtFile.FullName);
                        _changeTable[UPDATE_TABLE].Add(srcFile.FullName, updateList);
                    }
                    queryTgtIntersectSrc.RemoveAt(i);
                    queryTgtIntersectSrc.Insert(i, srcFile);
                    if (!propogateTable.ContainsKey(querySrcIntersectTgt[i].FullName))
                    {
                        propogateTable.Add(querySrcIntersectTgt[i].FullName, sourcePath);
                    }

                }
                else if (compareResult < 0)
                {
                    if (_changeTable[UPDATE_TABLE].ContainsKey(srcFile.FullName))
                    {
                        _changeTable[UPDATE_TABLE].Remove(srcFile.FullName);
                    }
                    if (propogateTable.ContainsKey(srcFile.FullName))
                    {
                        propogateTable.Remove(srcFile.FullName);
                    }
                }

            }

            return (queryTgtIntersectSrc.Union(querySrcExceptTgt)).Union(target).ToList<FileInfo>();
        }

        public List<FileInfo> GetAllDirectoryFiles(string path)
        {
            return new DirectoryInfo(path).GetFiles("*", SearchOption.AllDirectories).ToList<FileInfo>();
        }

        private string GetOriginFolder(string sourceFolder, string fullFilePath)
        {
            if (fullFilePath.Contains(sourceFolder))
            {
                return sourceFolder;
            }
            else
            {
                Debug.Assert(propogateTable.ContainsKey(fullFilePath));
                return propogateTable[fullFilePath];
            }
        }

        private string CreateNewItemPath(string sourceFolder, string fullFilePath, string targetOrigin)
        {
            String sourceOrigin = GetOriginFolder(sourceFolder, fullFilePath);
            Debug.Assert(fullFilePath.Contains(sourceOrigin));
            String rPath = fullFilePath.Substring(sourceOrigin.Length + 1);
            return Path.Combine(targetOrigin, rPath);
        }

        private List<CompareResult> ProcessRawResults()
        {
            Dictionary<int, Dictionary<string, List<string>>>.KeyCollection keys = _changeTable.Keys;
            Dictionary<string, List<string>>.KeyCollection currTableKeys = null;
            List<CompareResult> results = new List<CompareResult>();
            FileChangeType changeType = FileChangeType.Create;

            foreach (int key in keys)
            {
                currTableKeys = _changeTable[key].Keys;
                switch (key)
                {
                    case CREATE_TABLE:
                        changeType = FileChangeType.Create;
                        break;
                    case DELETE_TABLE:
                        changeType = FileChangeType.Delete;
                        break;
                    case RENAME_TABLE:
                        changeType = FileChangeType.Rename;
                        break;
                    case UPDATE_TABLE:
                        changeType = FileChangeType.Update;
                        break;
                }

                foreach (string sourceKey in currTableKeys)
                {
                    foreach (string dest in _changeTable[key][sourceKey])
                    {
                        results.Add(new CompareResult(changeType, sourceKey, dest));
                    }
                }
            }

            foreach (CompareResult cr in results)
            {

                Console.WriteLine(cr.ToString());
                Console.WriteLine();
            }

            return results;
        }

        /// <summary>
        /// Simple file name comparer.
        /// TODO: Compare by relative path to origin folder
        /// </summary>
        private class FileNameCompare : IEqualityComparer<FileInfo>
        {
            public bool Equals(FileInfo f1, FileInfo f2)
            {
                return (f1.Name.ToLower() == f2.Name.ToLower());
            }

            public int GetHashCode(FileInfo f)
            {
                return f.Name.ToLower().GetHashCode();
            }
        }

        class FileContentCompare : IComparer<FileInfo>
        {
            public int Compare(FileInfo f1, FileInfo f2)
            {
                bool isEqual = true;

                // YC: If file length is different, they are definitely different files
                if (f1.Length != f2.Length)
                    isEqual = false;

                if (isEqual)
                {
                    if (!f1.LastWriteTime.Equals(f2.LastWriteTime))
                    {
                        isEqual = false;
                    }

                    if (!isEqual)
                    {
                        FileStream fileStream1 = f1.OpenRead();
                        FileStream fileStream2 = f2.OpenRead();
                        byte[] fileHash1 = MD5.Create().ComputeHash(fileStream1);
                        byte[] fileHash2 = MD5.Create().ComputeHash(fileStream2);
                        fileStream1.Close();
                        fileStream2.Close();

                        for (int i = 0; i < fileHash1.Length; i++)
                        {
                            if (fileHash1[i] != fileHash2[i])
                            {
                                isEqual = false;
                                break;
                            }
                        }
                    }
                }

                if (isEqual)
                {
                    return 0;
                }
                else
                {
                    return f1.LastWriteTime.CompareTo(f2.LastWriteTime);
                }
            }
        }
    }
}
