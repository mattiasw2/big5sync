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
        private List<string> deleteList;
        private const string METADATAPATH = "_syncless\\metadata.xml";

        private List<CompareInfoObject> GetMonitorCompareObjects(List<MonitorPathPair> paths)
        {
            List<CompareInfoObject> results = new List<CompareInfoObject>();
            foreach (MonitorPathPair path in paths)
            {
                FileInfo f = new FileInfo(path.FullPath);
                results.Add(new CompareInfoObject(f.FullName, f.Name, f.CreationTime.Ticks, f.LastWriteTime.Ticks, f.Length, CalculateMD5Hash(f)));
            }
            return results;
        }

        public void MonitorCompareFolder(MonitorSyncRequest syncRequest, out List<string> paths, out List<CompareResult> results)
        {
            DirectoryInfo oldPath = new DirectoryInfo(syncRequest.OldPath.FullPath);
            DirectoryInfo newPath = null;

            if (syncRequest.NewPath != null)
            {
                newPath = new DirectoryInfo(syncRequest.NewPath.FullPath);
            }
            paths = new List<string>();
            results = new List<CompareResult>();

            switch (syncRequest.ChangeType)
            {
                case FileChangeType.Create:
                    foreach (MonitorPathPair dest in syncRequest.Dest)
                    {
                        results.Add(new CompareResult(syncRequest.ChangeType, syncRequest.OldPath.FullPath, dest.FullPath, syncRequest.IsFolder));
                    }
                    break;
                case FileChangeType.Delete:
                    //TO BE DONE
                    break;
                case FileChangeType.Rename:
                    DirectoryInfo folder = new DirectoryInfo(syncRequest.NewPath.FullPath);
                    string folderName = folder.Name;
                    foreach (MonitorPathPair dest in syncRequest.Dest)
                    {
                        string newDestPath = new DirectoryInfo(dest.FullPath).Parent.Name;
                        Console.WriteLine(newDestPath);
                        results.Add(new CompareResult(syncRequest.ChangeType, dest.FullPath, Path.Combine(newDestPath, folderName), syncRequest.IsFolder));
                    }
                    break;
            }

            Debug.Assert(syncRequest.OldPath != null);
            paths.AddRange(syncRequest.OldPath.Origin);

            if (syncRequest.NewPath != null)
            {
                paths.AddRange(syncRequest.NewPath.Origin);
            }

            foreach (MonitorPathPair dest in syncRequest.Dest)
            {
                paths.AddRange(dest.Origin);
            }

            paths.Distinct<string>();
        }

        public void MonitorCompareFile(MonitorSyncRequest syncRequest, out List<string> paths, out List<CompareResult> results)
        {
            FileInfo oldPath = new FileInfo(syncRequest.OldPath.FullPath);
            FileInfo newPath = null;            

            if (syncRequest.NewPath != null)
            {
                newPath = new FileInfo(syncRequest.NewPath.FullPath);
            }
            paths = new List<string>();
            results = new List<CompareResult>();

            if (syncRequest.OldPath.FullPath.EndsWith(METADATAPATH) || (syncRequest.NewPath != null && syncRequest.NewPath.FullPath.EndsWith(METADATAPATH)))
            {
                return;
            }

            CompareInfoObject source = null;
            List<CompareInfoObject> dests = null;

            switch (syncRequest.ChangeType)
            {
                case FileChangeType.Create:
                    source = new CompareInfoObject(oldPath.FullName, oldPath.Name, oldPath.CreationTime.Ticks, oldPath.LastWriteTime.Ticks, oldPath.Length, CalculateMD5Hash(oldPath));
                    foreach (MonitorPathPair dest in syncRequest.Dest)
                    {
                        results.Add(new CompareResult(syncRequest.ChangeType, syncRequest.OldPath.FullPath, dest.FullPath, syncRequest.IsFolder));
                    }
                    break;
                case FileChangeType.Update:
                    source = new CompareInfoObject(oldPath.FullName, oldPath.Name, oldPath.CreationTime.Ticks, oldPath.LastWriteTime.Ticks, oldPath.Length, CalculateMD5Hash(oldPath));
                    dests = GetMonitorCompareObjects(syncRequest.Dest);
                    foreach (CompareInfoObject dest in dests)
                    {
                        int compareResult = new FileContentCompare().Compare(source, dest);
                        if (compareResult != 0)
                        {
                            results.Add(new CompareResult(syncRequest.ChangeType, syncRequest.OldPath.FullPath, dest.FullName, syncRequest.IsFolder));
                        }
                    }
                    break;
                case FileChangeType.Delete:
                    dests = GetMonitorCompareObjects(syncRequest.Dest);
                    foreach (CompareInfoObject dest in dests)
                    {
                        results.Add(new CompareResult(syncRequest.ChangeType, dest.FullName, syncRequest.IsFolder));
                    }
                    break;
                case FileChangeType.Rename:
                    FileInfo file = new FileInfo(syncRequest.NewPath.FullPath);
                    string fileName = file.Name;
                    foreach (MonitorPathPair dest in syncRequest.Dest)
                    {
                        string newDestPath = new FileInfo(dest.FullPath).DirectoryName;
                        Console.WriteLine(newDestPath);
                        results.Add(new CompareResult(syncRequest.ChangeType, dest.FullPath, Path.Combine(newDestPath, fileName), syncRequest.IsFolder));
                    }
                    break;
            }

            Debug.Assert(syncRequest.OldPath != null);
            paths.AddRange(syncRequest.OldPath.Origin);

            if (syncRequest.NewPath != null)
            {
                paths.AddRange(syncRequest.NewPath.Origin);
            }

            foreach (MonitorPathPair dest in syncRequest.Dest)
            {
                paths.AddRange(dest.Origin);
            }

            paths.Distinct<string>();
        }
        
        // Assumes that all paths taken from the tag exists in the directory
        public List<CompareResult> CompareFile(List<string> paths)
        {
            List<CompareResult> compareResultList = new List<CompareResult>();
            FileInfo sourceInfo = null;
            FileInfo destInfo = null;

            for (int i = 0; i < paths.Count - 1; i++)
            {
                for (int j = 1; j <= paths.Count - 1; j++)
                {
                    sourceInfo = new FileInfo(paths[i]);
                    destInfo = new FileInfo(paths[j]);

                    //same content but different name (RENAME)
                    if(CalculateMD5Hash(sourceInfo).Equals(CalculateMD5Hash(destInfo))
                        && !sourceInfo.Name.Equals(destInfo.Name)) // same content different name
                    {
                        compareResultList.Add(new CompareResult(FileChangeType.Rename, sourceInfo.FullName,
                            destInfo.FullName, false));
                    }    
                    //different hash , but same name and creation time
                    else if (sourceInfo.Name.Equals(destInfo.Name) && sourceInfo.CreationTime.Ticks == destInfo.CreationTime.Ticks
                      && !CalculateMD5Hash(sourceInfo).Equals(CalculateMD5Hash(destInfo)))
                    {

                        compareResultList.Add(new CompareResult(FileChangeType.Update, sourceInfo.FullName,
                            destInfo.FullName, false));
                    }
                    else if (!(CalculateMD5Hash(sourceInfo).Equals(CalculateMD5Hash(destInfo)) &&
                        sourceInfo.CreationTime.Ticks == destInfo.CreationTime.Ticks && sourceInfo.Name.Equals(destInfo.Name)))
                    {
                        compareResultList.Add(new CompareResult(FileChangeType.Create, sourceInfo.FullName,
                            destInfo.FullName, false));
                    }
                        // same file with same content  NONE
                    else if (CalculateMD5Hash(sourceInfo).Equals(CalculateMD5Hash(destInfo)) &&
                        sourceInfo.Name.Equals(destInfo.Name))
                    {
                        compareResultList.Add(new CompareResult(FileChangeType.None, sourceInfo.FullName,
                            destInfo.FullName, false));
                    }

                }
            }

            return compareResultList;
        }

        public List<CompareResult> CompareFolder(List<string> paths)
        {
            _changeTable = new Dictionary<int, Dictionary<string, List<string>>>();
            _changeTable.Add(CREATE_TABLE, new Dictionary<string, List<string>>());
            _changeTable.Add(UPDATE_TABLE, new Dictionary<string, List<string>>());
            _changeTable.Add(RENAME_TABLE, new Dictionary<string, List<string>>());
            deleteList = new List<string>();

            List<string> withMeta = new List<string>();
            List<string> noMeta = new List<string>();

            foreach (string path in paths)
            {
                if (File.Exists(Path.Combine(path, XMLHelper.METADATAPATH)))
                {
                    withMeta.Add(path);
                }
                else
                {
                    noMeta.Add(path);
                }
            }

            List<CompareInfoObject> mostUpdated = null;

            // YC: If there is only 1 folder with metadata we have nothing to compare
            if (withMeta.Count < 2)
            {
                mostUpdated = DoRawCompareFolder(paths);
            }
            else
            {
                // YC: We compare the folders with metadata first, then
                // we do a raw compare for folders without metadata
                mostUpdated = DoOptimizedCompareFolder(withMeta, noMeta);
            }

            return ProcessRawResults(paths);
        }

        /// <summary>
        /// Compare folders against their respective metadata first, then proceed
        /// to compare them against one another.
        /// </summary>
        /// <param name="withMeta">List of folders with metadata</param>
        /// <param name="noMeta">LIst of folders without metadata</param>
        /// <returns>The most updated files across all folders</returns>
        private List<CompareInfoObject> DoOptimizedCompareFolder(List<string> withMeta, List<string> noMeta)
        {
            // YC: Handle metadata and differences
            List<CompareInfoObject> currSrcFolder = GetDiffMetaActual(withMeta[0]);

            for (int i = 1; i < withMeta.Count; i++)
            {
                currSrcFolder = DoOptimizedOneWayFolder(currSrcFolder, GetDiffMetaActual(withMeta[i]), withMeta[i]);
            }
            for (int i = withMeta.Count - 2; i >= 0; i--)
            {
                currSrcFolder = DoOptimizedOneWayFolder(currSrcFolder, GetDiffMetaActual(withMeta[i]), withMeta[i]);
            }

            if (noMeta.Count > 0)
            {
                currSrcFolder = currSrcFolder.Union(GetAllCompareObjects(withMeta[0]), new FileNameCompare()).ToList<CompareInfoObject>();

                for (int i = 0; i < noMeta.Count; i++)
                {
                    currSrcFolder = DoRawOneWayCompareFolder(currSrcFolder, GetAllCompareObjects(noMeta[i]), noMeta[i], null);
                }

                int loopStart = 0;

                if (noMeta.Count == 1)
                {
                    loopStart = noMeta.Count - 1;
                }
                else
                {
                    loopStart = noMeta.Count - 2;
                }

                for (int i = loopStart; i >= 0; i--)
                {
                    currSrcFolder = DoRawOneWayCompareFolder(currSrcFolder, GetAllCompareObjects(noMeta[i]), noMeta[i], withMeta);
                }

                currSrcFolder = DoRawOneWayCompareFolder(currSrcFolder, GetAllCompareObjects(withMeta[0]), withMeta[0], withMeta);
            }            
             
            return currSrcFolder;
        }

        private List<CompareInfoObject> GetDiffMetaActual(string path)
        {
            //Do some processing between meta and actual files
            List<CompareInfoObject> results = new List<CompareInfoObject>();

            List<CompareInfoObject> actual = GetAllCompareObjects(path);
            List<CompareInfoObject> meta = GetMetadataCompareObjects(path);

            // YC: This will give us files that exist but are not in the
            // metadata. Implies either new or renamed files.
            List<CompareInfoObject> actualExceptMeta = actual.Except<CompareInfoObject>(meta, new FileNameCompare()).ToList<CompareInfoObject>();

            // YC: This will give us files that exist in metadata but in the
            // actual folder. Implies either deleted or renamed files.
            List<CompareInfoObject> metaExceptActual = meta.Except<CompareInfoObject>(actual, new FileNameCompare()).ToList<CompareInfoObject>();
            bool rename = false;
            List<string> renameList = null;
            CompareInfoObject tempObject = null;

            foreach (CompareInfoObject a in actualExceptMeta)
            {
                rename = false;
                foreach (CompareInfoObject m in meta)
                {
                    if (a.MD5Hash == m.MD5Hash && a.CreationTime == m.CreationTime)
                    {
                        rename = true;
                        tempObject = m;
                        break;
                    }
                }
                if (rename)
                {
                    if (_changeTable[RENAME_TABLE].TryGetValue(tempObject.RelativePathToOrigin, out renameList))
                    {
                        if (!renameList.Contains(a.RelativePathToOrigin))
                        {
                            renameList.Add(a.RelativePathToOrigin);
                        }
                    }
                    else
                    {
                        renameList = new List<string>();
                        renameList.Add(a.RelativePathToOrigin);
                        _changeTable[RENAME_TABLE].Add(tempObject.RelativePathToOrigin, renameList);
                    }
                }
                else
                {
                    a.ChangeType = FileChangeType.Create;
                    results.Add(a);
                }
            }

            foreach (CompareInfoObject m in metaExceptActual)
            {
                rename = false;
                foreach (CompareInfoObject a in actual)
                {
                    if (a.MD5Hash == m.MD5Hash && a.CreationTime == m.CreationTime)
                    {
                        rename = true;
                        tempObject = a;
                        break;
                    }
                }
                if (rename)
                {
                    if (_changeTable[RENAME_TABLE].TryGetValue(m.RelativePathToOrigin, out renameList))
                    {
                        if (!renameList.Contains(tempObject.RelativePathToOrigin))
                        {
                            renameList.Add(tempObject.RelativePathToOrigin);
                        }
                    }
                    else
                    {
                        renameList = new List<string>();
                        renameList.Add(tempObject.RelativePathToOrigin);
                        _changeTable[RENAME_TABLE].Add(m.RelativePathToOrigin, renameList);
                    }
                }
                else
                {
                    if (!deleteList.Contains(m.RelativePathToOrigin)) {
                        deleteList.Add(m.RelativePathToOrigin);
                    }
                }
            }

            List<CompareInfoObject> actualIntersectMeta = actual.Intersect<CompareInfoObject>(meta, new FileNameCompare()).ToList<CompareInfoObject>();
            List<CompareInfoObject> metaIntersectActual = meta.Intersect<CompareInfoObject>(actual, new FileNameCompare()).ToList<CompareInfoObject>();
            actualIntersectMeta.Sort();
            metaIntersectActual.Sort();
            
            Debug.Assert(actualIntersectMeta.Count == metaIntersectActual.Count);
            int numOfCommonItems = actualIntersectMeta.Count;
            CompareInfoObject actualFile = null;
            CompareInfoObject metaFile = null;
            int compareResult = 0;

            for (int i = 0; i < numOfCommonItems; i++)
            {
                actualFile = (CompareInfoObject)actualIntersectMeta[i];
                metaFile = (CompareInfoObject)metaIntersectActual[i];
                Debug.Assert(actualFile.RelativePathToOrigin == metaFile.RelativePathToOrigin);
                compareResult = new FileContentCompare().Compare(actualFile, metaFile);

                if (actualFile.Length != metaFile.Length)
                {
                    actualFile.ChangeType = FileChangeType.Update;
                    results.Add(actualFile);
                    continue;
                }
                if (actualFile.MD5Hash != metaFile.MD5Hash)
                {
                    actualFile.ChangeType = FileChangeType.Update;
                    results.Add(actualFile);
                    continue;
                }
            }
            return results;
        }

        private List<CompareInfoObject> DoOptimizedOneWayFolder(List<CompareInfoObject> source, List<CompareInfoObject> target, string targetPath)
        {
            Debug.Assert(source != null && target != null);
            List<CompareInfoObject> querySrcExceptTgt = source.Except(target, new FileNameCompare()).ToList<CompareInfoObject>();
            List<CompareInfoObject> querySrcIntersectTgt = source.Intersect(target, new FileNameCompare()).ToList<CompareInfoObject>();
            List<CompareInfoObject> queryTgtIntersectSrc = target.Intersect(source, new FileNameCompare()).ToList<CompareInfoObject>();
            querySrcIntersectTgt.Sort();
            queryTgtIntersectSrc.Sort();

            Debug.Assert(querySrcIntersectTgt.Count == queryTgtIntersectSrc.Count);
            int exceptItemsCount = querySrcExceptTgt.Count;            
            int commonItemsCount = queryTgtIntersectSrc.Count;
            List<string> createList, updateList;

            for (int i = 0; i < exceptItemsCount; i++)
            {
                if (querySrcExceptTgt[i].ChangeType == FileChangeType.Create)
                {
                    if (_changeTable[CREATE_TABLE].TryGetValue(querySrcExceptTgt[i].FullName, out createList))
                    {
                        if (!createList.Contains(CreateNewItemPath(querySrcExceptTgt[i], targetPath)))
                        {
                            createList.Add(CreateNewItemPath(querySrcExceptTgt[i], targetPath));
                        }
                    }
                    else
                    {
                        createList = new List<string>();
                        createList.Add(CreateNewItemPath(querySrcExceptTgt[i], targetPath));
                        _changeTable[CREATE_TABLE].Add(querySrcExceptTgt[i].FullName, createList);
                    }
                }
                else if (querySrcExceptTgt[i].ChangeType == FileChangeType.Update)
                {
                    if (_changeTable[UPDATE_TABLE].TryGetValue(querySrcExceptTgt[i].FullName, out updateList))
                    {
                        if (!updateList.Contains(CreateNewItemPath(querySrcExceptTgt[i], targetPath)))
                        {
                            updateList.Add(CreateNewItemPath(querySrcExceptTgt[i], targetPath));
                        }
                    }
                    else
                    {
                        updateList = new List<string>();
                        updateList.Add(CreateNewItemPath(querySrcExceptTgt[i], targetPath));
                        _changeTable[UPDATE_TABLE].Add(querySrcExceptTgt[i].FullName, updateList);
                    }

                    //YC: Experimental
                    if (_changeTable[CREATE_TABLE].ContainsKey(querySrcExceptTgt[i].FullName))
                    {
                        _changeTable[CREATE_TABLE].Remove(querySrcExceptTgt[i].FullName);
                    }
                }
            }

            CompareInfoObject srcFile = null;
            CompareInfoObject tgtFile = null;
            int compareResult = 0;

            for (int i = 0; i < commonItemsCount; i++)
            {
                srcFile = (CompareInfoObject)querySrcIntersectTgt[i];
                tgtFile = (CompareInfoObject)queryTgtIntersectSrc[i];
                Debug.Assert(srcFile.RelativePathToOrigin == tgtFile.RelativePathToOrigin);
                compareResult = new FileContentCompare().Compare(srcFile, tgtFile);

                if (compareResult > 0)
                {
                    Debug.Assert(srcFile.ChangeType == FileChangeType.Update && tgtFile.ChangeType == FileChangeType.Update);
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
            return (queryTgtIntersectSrc.Union(querySrcExceptTgt, new FileNameCompare())).Union(target, new FileNameCompare()).ToList<CompareInfoObject>();
        }

        /// <summary>
        /// Compare folders regardless of metadata. It can only handle update and create changes,
        /// not delete and rename since the metadata is not used.
        /// </summary>
        /// <param name="paths">List of folders</param>
        /// <returns>The most updated files across all folders</returns>
        private List<CompareInfoObject> DoRawCompareFolder(List<string> paths)
        {            
            List<CompareInfoObject> currSrcFolder = GetAllCompareObjects(paths[0]);

            for (int i = 1; i < paths.Count; i++)
            {
                currSrcFolder = DoRawOneWayCompareFolder(currSrcFolder, GetAllCompareObjects(paths[i]), paths[i]);
            }
            for (int i = paths.Count - 2; i >= 0; i--)
            {
                currSrcFolder = DoRawOneWayCompareFolder(currSrcFolder, GetAllCompareObjects(paths[i]), paths[i]);
            }

            return currSrcFolder;
        }

        private List<CompareInfoObject> DoRawOneWayCompareFolder(List<CompareInfoObject> source, List<CompareInfoObject> target, string targetPath)
        {
            return DoRawOneWayCompareFolder(source, target, targetPath, null);
        }

        private List<CompareInfoObject> DoRawOneWayCompareFolder(List<CompareInfoObject> source, List<CompareInfoObject> target, string targetPath, List<string> virtualPaths)
        {
            Debug.Assert(source != null && target != null);
            List<CompareInfoObject> querySrcExceptTgt = source.Except(target, new FileNameCompare()).ToList<CompareInfoObject>();
            List<CompareInfoObject> querySrcIntersectTgt = source.Intersect(target, new FileNameCompare()).ToList<CompareInfoObject>();
            List<CompareInfoObject> queryTgtIntersectSrc = target.Intersect(source, new FileNameCompare()).ToList<CompareInfoObject>();
            querySrcIntersectTgt.Sort();
            queryTgtIntersectSrc.Sort();

            int exceptItemsCount = querySrcExceptTgt.Count;
            Debug.Assert(querySrcIntersectTgt.Count == queryTgtIntersectSrc.Count);
            int commonItemsCount = queryTgtIntersectSrc.Count;
            List<string> createList, updateList;

            for (int i = 0; i < exceptItemsCount; i++)
            {
                List<string> newFilePaths = new List<string>();
                if (virtualPaths != null && virtualPaths.Contains(targetPath))
                {
                    foreach (string virtualPath in virtualPaths)
                    {
                        if (querySrcExceptTgt[i].Origin != virtualPath)
                        {
                            newFilePaths.Add(CreateNewItemPath(querySrcExceptTgt[i], virtualPath));
                        }
                    }
                }
                else
                {
                    newFilePaths.Add(CreateNewItemPath(querySrcExceptTgt[i], targetPath));
                }

                if (_changeTable[CREATE_TABLE].TryGetValue(querySrcExceptTgt[i].FullName, out createList))
                {
                    foreach (string newFilePath in newFilePaths)
                    {
                        if (!createList.Contains(newFilePath))
                        {
                            createList.Add(newFilePath);
                        }
                    }
                }
                else
                {
                    if (querySrcExceptTgt[i].Origin != targetPath)
                    {
                        createList = new List<string>();
                        foreach (string newFilePath in newFilePaths)
                        {
                            if (!createList.Contains(newFilePath))
                            {
                                createList.Add(newFilePath);
                            }
                        }
                        _changeTable[CREATE_TABLE].Add(querySrcExceptTgt[i].FullName, createList);
                    }
                }
            }

            CompareInfoObject srcFile = null;
            CompareInfoObject tgtFile = null;
            int compareResult = 0;

            for (int i = 0; i < commonItemsCount; i++)
            {
                srcFile = (CompareInfoObject)querySrcIntersectTgt[i];
                tgtFile = (CompareInfoObject)queryTgtIntersectSrc[i];
                Debug.Assert(srcFile.RelativePathToOrigin == tgtFile.RelativePathToOrigin);
                compareResult = new FileContentCompare().Compare(srcFile, tgtFile);

                if (compareResult > 0)
                {
                    List<string> updatePaths = new List<string>();
                    if (virtualPaths != null && virtualPaths.Contains(targetPath))
                    {
                        foreach (string virtualPath in virtualPaths)
                        {
                            if (srcFile.Origin != virtualPath)
                            {
                                updatePaths.Add(CreateNewItemPath(tgtFile, virtualPath));
                            }
                        }
                    }
                    else
                    {
                        updatePaths.Add(tgtFile.FullName);
                    }

                    if (_changeTable[UPDATE_TABLE].TryGetValue(srcFile.FullName, out updateList))
                    {
                        foreach (string updatePath in updatePaths)
                        {
                            if (!updateList.Contains(updatePath))
                            {
                                updateList.Add(updatePath);
                            }
                        }
                    }
                    else
                    {
                        updateList = new List<string>();
                        foreach (string updatePath in updatePaths)
                        {
                            updateList.Add(updatePath);
                        }
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
            return (queryTgtIntersectSrc.Union(querySrcExceptTgt, new FileNameCompare())).Union(target, new FileNameCompare()).ToList<CompareInfoObject>();
        }

        private List<CompareInfoObject> GetAllCompareObjects(string path)
        {
            FileInfo[] allFiles = new DirectoryInfo(path).GetFiles("*", SearchOption.AllDirectories);
            List<CompareInfoObject> results = new List<CompareInfoObject>();
            foreach (FileInfo f in allFiles)
            {
                if (f.Directory.Name != "_syncless")
                    results.Add(new CompareInfoObject(path, f.FullName, f.Name, f.CreationTime.Ticks, f.LastWriteTime.Ticks, f.Length, CalculateMD5Hash(f)));
            }
            return results;
        }

        /// <summary>
        /// Creates a list of CompareInfoObject based on metadata
        /// </summary>
        /// <param name="path"></param>
        /// <returns>List of CompareInfoObject</returns>
        private List<CompareInfoObject> GetMetadataCompareObjects(string path)
        {
            return XMLHelper.GetCompareInfoObjects(path);
        }

        private string CreateNewItemPath(CompareInfoObject source, string targetOrigin)
        {
            Debug.Assert(source != null && targetOrigin != null);            
            return Path.Combine(targetOrigin, source.RelativePathToOrigin);
        }

        /// <summary>
        /// Process all the hashtables and lists of results and returns a list of CompareResults
        /// </summary>
        /// <param name="paths"></param>
        /// <returns>List of CompareResults</returns>
        private List<CompareResult> ProcessRawResults(List<string> paths)
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
                    case RENAME_TABLE:
                        changeType = FileChangeType.Rename;
                        break;
                    case UPDATE_TABLE:
                        changeType = FileChangeType.Update;
                        break;
                }

                foreach (string sourceKey in currTableKeys)
                {
                    if (changeType == FileChangeType.Create || changeType == FileChangeType.Update)
                    {
                        foreach (string dest in _changeTable[key][sourceKey])
                        {
                            results.Add(new CompareResult(changeType, sourceKey, dest, false));
                        }
                    }
                    else if (changeType == FileChangeType.Rename)
                    {
                        List<string> rename = _changeTable[key][sourceKey];
                        if (rename.Count == 1)
                        {
                            foreach (string path in paths)
                            {
                                results.Add(new CompareResult(changeType, Path.Combine(path, sourceKey), Path.Combine(path, rename[0]), false));
                            }
                        }
                        // TODO: Handle rename conflicts in future
                        else if (rename.Count > 1)
                        {
                            foreach (string path in paths)
                            {
                                results.Add(new CompareResult(changeType, Path.Combine(path, sourceKey), Path.Combine(path, rename[0]), false));
                            }
                        }
                    }
                }
            }

            // TODO: Handle delete conflicts. Simple way for now.
            string deletePath = null;
            bool add = true;
            foreach (string deleteItem in deleteList)
            {
                add = true;
                foreach (string path in paths)
                {                    
                    deletePath = Path.Combine(path, deleteItem);

                    if (_changeTable[CREATE_TABLE].ContainsKey(deletePath) || _changeTable[UPDATE_TABLE].ContainsKey(deletePath) || _changeTable[RENAME_TABLE].ContainsKey(deleteItem))
                    {
                        add = false;
                    }                   
                }
                if (add)
                {
                    foreach (string path in paths)
                    {
                        deletePath = Path.Combine(path, deleteItem);
                        results.Add(new CompareResult(FileChangeType.Delete, deletePath, false));
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
        /// Calculates the MD5 hash from a FileInfo object
        /// </summary>
        /// <param name="fileInput">FileInfo object to be hashed</param>
        /// <returns>MD5 hash formatted in hexadecimal</returns>
        public static string CalculateMD5Hash(FileInfo fileInput)
        {
            Debug.Assert(fileInput.Exists);
            Debug.Assert(fileInput.Name != "syncless.xml");
            Debug.Assert(fileInput.Directory.Name != "_syncless");
            FileStream fileStream = fileInput.OpenRead();
            byte[] fileHash = MD5.Create().ComputeHash(fileStream);
            fileStream.Close();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < fileHash.Length; i++)
            {
                sb.Append(fileHash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Compares the relative path to origin of one folder to another and determine
        /// if they have the same path and name.
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

        /// <summary>
        /// Compares the length, last write time, and MD5 hash of the files to determine if they
        /// are equal
        /// </summary>
        class FileContentCompare : IComparer<CompareInfoObject>
        {
            public int Compare(CompareInfoObject c1, CompareInfoObject c2)
            {
                bool isEqual = true;

                // YC: If file length is different, they are definitely different files
                if (c1.Length != c2.Length)
                    isEqual = false;

                if (isEqual)
                {
                    if (!c1.LastWriteTime.Equals(c2.LastWriteTime))
                    {
                        isEqual = false;
                    }

                    if (!isEqual)
                    {
                        if (c1.MD5Hash == c2.MD5Hash)
                        {
                            isEqual = true;
                        }
                    }
                }

                if (isEqual)
                {
                    return 0;
                }
                else
                {
                    return c1.LastWriteTime.CompareTo(c2.LastWriteTime);
                }
            }
        }
    }
}
