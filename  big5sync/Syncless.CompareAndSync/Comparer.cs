using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using Syncless.Tagging;

namespace Syncless.CompareAndSync
{
    public class Comparer
    {
        private const int CREATE_TABLE = 0, DELETE_TABLE = 1, RENAME_TABLE = 2, UPDATE_TABLE = 3;
        private Dictionary<int, Dictionary<string, List<string>>> _changeTable;

        public List<CompareResult> CompareFolder(/*FolderTag fTag*/ List<string> paths)
        {
            //Debug.Assert(fTag != null);
            _changeTable = new Dictionary<int, Dictionary<string, List<string>>>();
            _changeTable.Add(CREATE_TABLE, new Dictionary<string, List<string>>());
            _changeTable.Add(DELETE_TABLE, new Dictionary<string, List<string>>());
            _changeTable.Add(RENAME_TABLE, new Dictionary<string, List<string>>());
            _changeTable.Add(UPDATE_TABLE, new Dictionary<string, List<string>>());

            //YC: Please change this soon, Eric. I should not be processing TaggedPaths.
            /*
            List<TaggedPath> taggedPaths = fTag.FolderPaths;
            List<string> paths = null;
            foreach (TaggedPath path in taggedPaths)
            {
                paths.Add(path.Path);
            }
             */
            
            List<CompareInfoObject> currSrcFolder = GetAllCompareObjects(paths[0]);

            for (int i = 1; i < paths.Count; i++)
            {
                currSrcFolder = OneWayCompareFolder(currSrcFolder, GetAllCompareObjects(paths[i]), paths[i]);
            }
            for (int i = paths.Count - 2; i >= 0; i--)
            {
                currSrcFolder = OneWayCompareFolder(currSrcFolder, GetAllCompareObjects(paths[i]), paths[i]);
            }
            ProcessRawResults();
            return null;
        }

        public List<CompareInfoObject> OneWayCompareFolder(List<CompareInfoObject> source, List<CompareInfoObject> target, string targetPath)
        {
            Debug.Assert(source != null && target != null);
            List<CompareInfoObject> querySrcExceptTgt = source.Except(target, new FileNameCompare()).ToList<CompareInfoObject>();
            List<CompareInfoObject> querySrcIntersectTgt = source.Intersect(target, new FileNameCompare()).ToList<CompareInfoObject>();
            List<CompareInfoObject> queryTgtIntersectSrc = target.Intersect(source, new FileNameCompare()).ToList<CompareInfoObject>();
            int exceptItemsCount = querySrcExceptTgt.Count;
            Debug.Assert(querySrcIntersectTgt.Count == queryTgtIntersectSrc.Count);
            int commonItemsCount = queryTgtIntersectSrc.Count;
            List<string> createList, deleteList, renameList, updateList;
            string newFilePath = null;

            for (int i = 0; i < exceptItemsCount; i++)
            {
                newFilePath = CreateNewItemPath(querySrcExceptTgt[i], targetPath);
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
            }

            for (int i = 0; i < commonItemsCount; i++)
            {
                CompareInfoObject srcFile = (CompareInfoObject)querySrcIntersectTgt[i];
                CompareInfoObject tgtFile = (CompareInfoObject)queryTgtIntersectSrc[i];
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

                    //YC: Experimental
                    if (_changeTable[CREATE_TABLE].ContainsKey(tgtFile.FullName))
                    {
                        _changeTable[CREATE_TABLE].Remove(tgtFile.FullName);
                    }

                    queryTgtIntersectSrc.RemoveAt(i);
                    queryTgtIntersectSrc.Insert(i, srcFile);
                }
                else if (compareResult < 0)
                {
                    if (_changeTable[UPDATE_TABLE].ContainsKey(srcFile.FullName))
                    {
                        _changeTable[UPDATE_TABLE].Remove(srcFile.FullName);
                    }
                }

            }

            return (queryTgtIntersectSrc.Union(querySrcExceptTgt)).Union(target).ToList<CompareInfoObject>();
        }

        public List<CompareInfoObject> GetAllCompareObjects(string path)
        {
            FileInfo[] allFiles = new DirectoryInfo(path).GetFiles("*", SearchOption.AllDirectories);
            List<CompareInfoObject> results = new List<CompareInfoObject>();
            foreach (FileInfo f in allFiles)
            {
                results.Add(new CompareInfoObject(path, f.FullName, f.Name, f.LastWriteTime));
            }
            return results;
        }

        private string CreateNewItemPath(CompareInfoObject source, string targetOrigin)
        {
            Debug.Assert(source != null && targetOrigin != null);            
            return Path.Combine(targetOrigin, source.RelativePathToOrigin);
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
        private class FileNameCompare : IEqualityComparer<CompareInfoObject>
        {
            public bool Equals(CompareInfoObject c1, CompareInfoObject c2)
            {
                return (c1.RelativePathToOrigin.ToLower() == c2.RelativePathToOrigin.ToLower());
            }

            public int GetHashCode(CompareInfoObject c)
            {
                return c.RelativePathToOrigin.ToLower().GetHashCode();
            }
        }

        class FileContentCompare : IComparer<CompareInfoObject>
        {
            public int Compare(CompareInfoObject c1, CompareInfoObject c2)
            {
                FileInfo f1 = new FileInfo(c1.FullName);
                FileInfo f2 = new FileInfo(c2.FullName);

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
