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
        private Dictionary<CompareInfoObject, List<string>> createTable, updateTable;
        private Dictionary<string, List<string>> renameTable;
        private List<string> deleteList;
        private const string METADATAPATH = "_syncless\\metadata.xml";

        #region Monitor

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
                        results.Add(FolderCompareResult.NewFolderCompareResult(syncRequest.OldPath.FullPath, dest.FullPath));
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
                        string newDestPath = new DirectoryInfo(dest.FullPath).Parent.FullName;
                        Console.WriteLine(newDestPath);
                        results.Add(FolderCompareResult.RenameFolderCompareResult(dest.FullPath, Path.Combine(newDestPath, folderName)));
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
                        results.Add(FileCompareResult.CreateFileCompareResult(syncRequest.OldPath.FullPath, dest.FullPath, CalculateMD5Hash(new FileInfo(syncRequest.OldPath.FullPath)), 0, 0, 0));
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
                            results.Add(FileCompareResult.UpdateFileCompareResult(syncRequest.OldPath.FullPath, dest.FullName, CalculateMD5Hash(new FileInfo(syncRequest.OldPath.FullPath)), 0, 0, 0));
                        }
                    }
                    break;
                case FileChangeType.Delete:
                    dests = GetMonitorCompareObjects(syncRequest.Dest);
                    foreach (CompareInfoObject dest in dests)
                    {
                        results.Add(FileCompareResult.DeleteFileCompareResult(dest.FullName));
                    }
                    break;
                case FileChangeType.Rename:
                    FileInfo file = new FileInfo(syncRequest.NewPath.FullPath);
                    string fileName = file.Name;
                    foreach (MonitorPathPair dest in syncRequest.Dest)
                    {
                        string newDestPath = new FileInfo(dest.FullPath).DirectoryName;
                        Console.WriteLine(newDestPath);
                        results.Add(FileCompareResult.RenameFileCompareResult(dest.FullPath, Path.Combine(newDestPath, fileName)));
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

        #endregion

        #region Manual

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
                        compareResultList.Add(FileCompareResult.RenameFileCompareResult(sourceInfo.FullName,
                            destInfo.FullName));
                    }    
                    //different hash , but same name and creation time
                    else if (sourceInfo.Name.Equals(destInfo.Name) && sourceInfo.CreationTime.Ticks == destInfo.CreationTime.Ticks
                      && !CalculateMD5Hash(sourceInfo).Equals(CalculateMD5Hash(destInfo)))
                    {
                        compareResultList.Add(FileCompareResult.UpdateFileCompareResult(sourceInfo.FullName,
                            destInfo.FullName, (CalculateMD5Hash(sourceInfo)), sourceInfo.CreationTime.Ticks, sourceInfo.LastWriteTime.Ticks, sourceInfo.Length));
                    }
                    /*
                    else if (!(CalculateMD5Hash(sourceInfo).Equals(CalculateMD5Hash(destInfo)) &&
                        sourceInfo.CreationTime.Ticks == destInfo.CreationTime.Ticks && sourceInfo.Name.Equals(destInfo.Name)))
                    {
                        compareResultList.Add(FileCompareResult(FileChangeType.Create, sourceInfo.FullName,
                            destInfo.FullName, false, (CalculateMD5Hash(sourceInfo))));
                    }
                    /*
                    else if (CalculateMD5Hash(sourceInfo).Equals(CalculateMD5Hash(destInfo)) &&
                        sourceInfo.Name.Equals(destInfo.Name))
                    {
                        compareResultList.Add(new CompareResult(FileChangeType.None, sourceInfo.FullName,
                            destInfo.FullName, false));
                    }*/

                }
            }

            return compareResultList;
        }

        public List<CompareResult> CompareFolder(List<string> paths)
        {
            createTable = new Dictionary<CompareInfoObject, List<string>>();
            updateTable = new Dictionary<CompareInfoObject, List<string>>();
            renameTable = new Dictionary<string, List<string>>();
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

        #endregion

        #region Optimized Compare Folder

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
                if (querySrcExceptTgt[i].ChangeType == FileChangeType.Create || querySrcExceptTgt[i].ChangeType == FileChangeType.None)
                {
                    if (createTable.TryGetValue(querySrcExceptTgt[i], out createList))
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
                        createTable.Add(querySrcExceptTgt[i], createList);
                    }
                }
                else if (querySrcExceptTgt[i].ChangeType == FileChangeType.Update)
                {
                    if (updateTable.TryGetValue(querySrcExceptTgt[i], out updateList))
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
                        updateTable.Add(querySrcExceptTgt[i], updateList);
                    }

                    //YC: Experimental
                    if (createTable.ContainsKey(querySrcExceptTgt[i]))
                    {
                        createTable.Remove(querySrcExceptTgt[i]);
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
                    //Debug.Assert(srcFile.ChangeType == FileChangeType.Update && tgtFile.ChangeType == FileChangeType.Update);
                    if (updateTable.TryGetValue(srcFile, out updateList))
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
                        updateTable.Add(srcFile, updateList);
                    }

                    //YC: Experimental
                    if (createTable.ContainsKey(tgtFile))
                    {
                        createTable.Remove(tgtFile);
                    }

                    queryTgtIntersectSrc.RemoveAt(i);
                    queryTgtIntersectSrc.Insert(i, srcFile);
                }
                else if (compareResult < 0)
                {
                    if (updateTable.ContainsKey(srcFile))
                    {
                        updateTable.Remove(srcFile);
                    }
                }

            }
            return (queryTgtIntersectSrc.Union(querySrcExceptTgt, new FileNameCompare())).Union(target, new FileNameCompare()).ToList<CompareInfoObject>();
        }

        #endregion

        #region Raw Compare Folder

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

        /// <summary>
        /// Compare 2 folders in one direction regardless of metadata. As a result, it can only handle
        /// creations and modifications, not deletions or renames.
        /// </summary>
        /// <param name="source">List of files/folders from source folder</param>
        /// <param name="target">List of files/folders in target folder</param>
        /// <param name="targetPath"></param>
        /// <param name="virtualPaths"></param>
        /// <returns></returns>
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

                if (createTable.TryGetValue(querySrcExceptTgt[i], out createList))
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
                        createTable.Add(querySrcExceptTgt[i], createList);
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

                    if (updateTable.TryGetValue(srcFile, out updateList))
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
                        updateTable.Add(srcFile, updateList);
                    }

                    //YC: Experimental
                    if (createTable.ContainsKey(tgtFile))
                    {
                        createTable.Remove(tgtFile);
                    }

                    queryTgtIntersectSrc.RemoveAt(i);
                    queryTgtIntersectSrc.Insert(i, srcFile);
                }
                else if (compareResult < 0)
                {
                    if (updateTable.ContainsKey(srcFile))
                    {
                        updateTable.Remove(srcFile);
                    }
                }

            }
            return (queryTgtIntersectSrc.Union(querySrcExceptTgt, new FileNameCompare())).Union(target, new FileNameCompare()).ToList<CompareInfoObject>();
        }

        #endregion

        #region Get CompareInfoObject Methods

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
        /// Returns a list of files to be checked
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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
                    if (renameTable.TryGetValue(tempObject.RelativePathToOrigin, out renameList))
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
                        renameTable.Add(tempObject.RelativePathToOrigin, renameList);
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
                    if (renameTable.TryGetValue(m.RelativePathToOrigin, out renameList))
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
                        renameTable.Add(m.RelativePathToOrigin, renameList);
                    }
                }
                else
                {
                    if (!deleteList.Contains(m.RelativePathToOrigin))
                    {
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
                }
                else if (actualFile.MD5Hash != metaFile.MD5Hash)
                {
                    actualFile.ChangeType = FileChangeType.Update;
                    results.Add(actualFile);
                }
                else
                {
                    results.Add(actualFile);
                }

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

        #endregion

        #region Helper Methods

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
            FileStream fileStream = null;
            try
            {
                fileStream = fileInput.OpenRead();
            }
            catch (IOException)
            {
                fileInput.Refresh();
                fileStream = fileInput.OpenRead();
            }

            byte[] fileHash = MD5.Create().ComputeHash(fileStream);
            fileStream.Close();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < fileHash.Length; i++)
            {
                sb.Append(fileHash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        private string CreateNewItemPath(CompareInfoObject source, string targetOrigin)
        {
            Debug.Assert(source != null && targetOrigin != null);            
            return Path.Combine(targetOrigin, source.RelativePathToOrigin);
        }

        private string GetDirectoryNameChange(CompareInfoObject c1, CompareInfoObject c2)
        {
            FileInfo f1 = new FileInfo(c1.FullName);
            FileInfo f2 = new FileInfo(c2.FullName);
            DirectoryInfo currDir1 = null, currDir2 = null;
            
            currDir1 = f1.Directory;
            currDir2 = f2.Directory;
            
            do
            {
                currDir1 = currDir1.Parent;
                currDir2 = currDir2.Parent;
            }
            while (currDir1.Name == currDir2.Name);

            return currDir1.Name;
        }

        /// <summary>
        /// Process all the hashtables and lists of results and returns a list of CompareResults
        /// </summary>
        /// <param name="paths"></param>
        /// <returns>List of CompareResults</returns>
        private List<CompareResult> ProcessRawResults(List<string> paths)
        {
            Dictionary<CompareInfoObject, List<string>>.KeyCollection createUpdateKeys = null;
            Dictionary<string, List<string>>.KeyCollection renameKeys = null;
            List<CompareResult> results = new List<CompareResult>();
            renameKeys = renameTable.Keys;

            // Handle Create Table
            createUpdateKeys = createTable.Keys;
            foreach (CompareInfoObject sourceKey in createUpdateKeys)
            {
                // Ensure that a file is not created cause it was renamed
                if (!renameKeys.Contains(sourceKey.RelativePathToOrigin))
                {
                    foreach (string dest in createTable[sourceKey])
                    {
                        results.Add(FileCompareResult.CreateFileCompareResult(sourceKey.FullName, dest, sourceKey.MD5Hash, sourceKey.CreationTime, sourceKey.LastWriteTime, sourceKey.Length));
                    }
                }
            }

            // Handle Update Table
            createUpdateKeys = updateTable.Keys;
            foreach (CompareInfoObject sourceKey in createUpdateKeys)
            {
                foreach (string dest in updateTable[sourceKey])
                {
                    results.Add(FileCompareResult.UpdateFileCompareResult(sourceKey.FullName, dest, sourceKey.MD5Hash, sourceKey.CreationTime, sourceKey.LastWriteTime, sourceKey.Length));
                }
            }

            // Handle Rename Table
            foreach (string sourceKey in renameKeys)
            {
                List<string> rename = renameTable[sourceKey];
                if (rename.Count == 1)
                {
                    foreach (string path in paths)
                    {
                        results.Add(FileCompareResult.RenameFileCompareResult(Path.Combine(path, sourceKey), Path.Combine(path, rename[0])));
                    }
                }
                // TODO: Handle rename conflicts in future
                else if (rename.Count > 1)
                {
                    foreach (string path in paths)
                    {
                        results.Add(FileCompareResult.RenameFileCompareResult(Path.Combine(path, sourceKey), Path.Combine(path, rename[0])));
                    }
                }
            }

            string deletePath = null;
            bool add = true;
            foreach (string deleteItem in deleteList)
            {
                add = true;
                foreach (string path in paths)
                {
                    deletePath = Path.Combine(path, deleteItem);
                    foreach (CompareResult result in results)
                    {
                        if (result.From == deletePath)
                        {
                            add = false;
                            break;
                        }
                    }
                }
                if (add)
                {
                    foreach (string path in paths)
                    {
                        deletePath = Path.Combine(path, deleteItem);
                        results.Add(FileCompareResult.DeleteFileCompareResult(deletePath));
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

        #endregion

        #region Comparer Classes

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

        #endregion
    }
}
