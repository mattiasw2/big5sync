using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Redesign
{
    class Comparer
    {

        Dictionary<CompareObject, List<string>> createTable, updateTable;

        #region Raw Comparer

        public FolderCompareObject RawComparer(List<string> paths)
        {
            createTable = new Dictionary<CompareObject, List<string>>();
            updateTable = new Dictionary<CompareObject, List<string>>();

            FolderCompareObject currSrcFolder = new FolderCompareObject(paths[0], new DirectoryInfo(paths[0]));

            for (int i = 1; i < paths.Count; i++)
            {
                currSrcFolder = RawComparerHelper(currSrcFolder, new FolderCompareObject(paths[i], new DirectoryInfo(paths[i])));
            }
            for (int i = paths.Count - 2; i >= 0; i--)
            {
                currSrcFolder = RawComparerHelper(currSrcFolder, new FolderCompareObject(paths[i], new DirectoryInfo(paths[i])));
            }

            return currSrcFolder;
        }

        public FolderCompareObject RawComparerHelper(FolderCompareObject srcFolder, FolderCompareObject tgtFolder)
        {
            List<CompareObject> srcContents = srcFolder.Contents;
            List<CompareObject> tgtContents = tgtFolder.Contents;
            List<CompareObject> querySrcExceptTgt = srcContents.Except(tgtContents, new NameCompare()).ToList<CompareObject>();

            foreach (CompareObject item in querySrcExceptTgt)
            {
                List<string> createList = null;
                string newItemPath = CreateNewItemPath(item.RelativePathToOrigin, tgtFolder.Origin);
                if (createTable.TryGetValue(item, out createList))
                {
                    if (!createList.Contains(newItemPath))
                    {
                        createList.Add(newItemPath);
                    }
                }
                else
                {
                    createList = new List<string>();
                    createList.Add(newItemPath);
                    createTable.Add(item, createList);
                }
            }

            List<CompareObject> querySrcIntersectTgt = srcContents.Intersect(tgtContents, new NameCompare()).ToList<CompareObject>();
            List<CompareObject> queryTgtIntersectSrc = tgtContents.Intersect(srcContents, new NameCompare()).ToList<CompareObject>();
            Debug.Assert(querySrcIntersectTgt.Count == queryTgtIntersectSrc.Count);

            for (int i = 0; i < querySrcIntersectTgt.Count; i++)
            {
                if (querySrcIntersectTgt[i] is FolderCompareObject)
                {
                    queryTgtIntersectSrc[i] = RawComparerHelper((FolderCompareObject)querySrcIntersectTgt[i], (FolderCompareObject)queryTgtIntersectSrc[i]);
                }
                else if (querySrcIntersectTgt[i] is FileCompareObject)
                {
                    List<string> updateList = null;
                    long diff = CompareFileContents((FileCompareObject)querySrcIntersectTgt[i], (FileCompareObject)queryTgtIntersectSrc[i]);
                    if (diff > 0)
                    {
                        if (updateTable.TryGetValue(querySrcIntersectTgt[i], out updateList))
                        {
                            if (!updateList.Contains(queryTgtIntersectSrc[i].FullName))
                            {
                                updateList.Add(queryTgtIntersectSrc[i].FullName);
                            }
                        }
                        else
                        {
                            updateList = new List<string>();
                            updateList.Add(queryTgtIntersectSrc[i].FullName);
                            updateTable.Add(querySrcIntersectTgt[i], updateList);
                        }

                        if (createTable.ContainsKey(queryTgtIntersectSrc[i]))
                        {
                            createTable.Remove(queryTgtIntersectSrc[i]);
                        }

                        queryTgtIntersectSrc.RemoveAt(i);
                        queryTgtIntersectSrc.Insert(i, querySrcIntersectTgt[i]);
                    }
                    else if (diff < 0)
                    {
                        if (updateTable.ContainsKey(querySrcIntersectTgt[i]))
                        {
                            updateTable.Remove(querySrcIntersectTgt[i]);
                        }
                    }
                }
            }

            tgtFolder.Contents = (queryTgtIntersectSrc.Union(querySrcExceptTgt, new NameCompare())).Union(tgtContents, new NameCompare()).ToList<CompareObject>();

            return tgtFolder;
        }

        #endregion

        #region Optimized Comparer


        #endregion

        #region Seamless Comparer

        public void MonitorComparer()
        {
        }

        private void MonitorFileComparer()
        {
        }

        private void MonitorFolderComparer()
        {
        }

        #endregion

        private string CreateNewItemPath(string sourceFile, string targetOrigin)
        {
            Debug.Assert(sourceFile != null && targetOrigin != null);
            return Path.Combine(targetOrigin, sourceFile);
        }

        private void ProcessRawResults()
        {
            

            if (createTable != null)
                createTable = null;
            if (updateTable != null)
                updateTable = null;
        }

        private long CompareFileContents(FileCompareObject f1, FileCompareObject f2)
        {
            int diff = f1.LastWriteTime.CompareTo(f2.LastWriteTime);

            if (f1.Length != f2.Length)
            {
                return diff;
            }

            if (f1.Hash != f2.Hash)
            {
                return diff;
            }

            return 0;
        }

        #region Comparer Classes

        private class NameCompare : IEqualityComparer<CompareObject>
        {
            public bool Equals(CompareObject c1, CompareObject c2)
            {
                return (c1.Name.ToLower() == c2.Name.ToLower());
            }

            public int GetHashCode(CompareObject c)
            {
                return c.Name.ToLower().GetHashCode();
            }
        }

        #endregion
    }
}
