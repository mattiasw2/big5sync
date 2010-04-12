using System;
using System.Collections.Generic;
using System.IO;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.Core;
using Syncless.Filters;
using Syncless.Logging;
using Syncless.Notification;

namespace Syncless.CompareAndSync.Manual.Visitor
{
    /// <summary>
    /// BuilderVisitor is responsible for creating the tree.
    /// </summary>
    public class BuilderVisitor : IVisitor
    {

        #region IVisitor Members

        private readonly List<Filter> _filter;
        private readonly FilterChain _filterChain;
        private readonly List<string> _typeConflicts;
        private readonly Progress _progress;

        /// <summary>
        /// Instantiates a <c>BuilderVisitor</c> object.
        /// </summary>
        /// <param name="filter">The list of filters to pass in.</param>
        /// <param name="typeConflicts">A list to keep track of invalid type conflicts.</param>
        /// <param name="progress">A progress object to keep track of the progress.</param>
        public BuilderVisitor(List<Filter> filter, List<string> typeConflicts, Progress progress)
        {
            _filter = filter;
            _filterChain = new FilterChain();
            _typeConflicts = typeConflicts;
            _progress = progress;
        }

        // Do nothing for file
        public void Visit(FileCompareObject file, int numOfPaths)
        {
        }

        /// <summary>
        /// Visit <see cref="FolderCompareObject"/> implementation for BuilderVisitor.
        /// </summary>
        /// <param name="folder">The <see cref="FolderCompareObject"/> to build and process.</param>
        /// <param name="numOfPaths">The total number of folders to keep in sync.</param>
        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            RootCompareObject root = folder as RootCompareObject;

            for (int index = 0; index < numOfPaths; index++)
            {
                string path = root == null ? Path.Combine(folder.GetSmartParentPath(index), folder.Name) : root.Paths[index];
                DirectoryInfo f = new DirectoryInfo(path);

                if (f.Exists)
                {
                    if (_progress != null)
                    {
                        _progress.Message = f.FullName;
                        _progress.Update();
                    }

                    ProcessFolders(folder, numOfPaths, f, index);
                    ProcessFiles(folder, numOfPaths, f, index);
                }
            }
        }

        /// <summary>
        /// Visit <see cref="RootCompareObject"/> implementation for BuilderVisitor.
        /// </summary>
        /// <param name="root">The <see cref="RootCompareObject"/> to build and process.</param>
        /// <remarks><see cref="RootCompareObject"/> is a specific type of <see cref="FolderCompareObject"/>, and is thus handled similarly.</remarks>
        public void Visit(RootCompareObject root)
        {
            Visit(root, root.Paths.Length);
        }

        #endregion

        #region Process Methods

        /// <summary>
        /// Processes and adds files to the tree.
        /// </summary>
        /// <param name="folder"><see cref="FolderCompareObject"/> to process and add files to.</param>
        /// <param name="numOfPaths">The total number of sync folders.</param>
        /// <param name="f">The <see cref="DirectoryInfo"/> to process and get files from.</param>
        /// <param name="index">The index indicating which sync folder it belongs to.</param>
        private void ProcessFiles(FolderCompareObject folder, int numOfPaths, DirectoryInfo f, int index)
        {
            FileInfo[] fileInfos = f.GetFiles();
            foreach (FileInfo info in fileInfos)
            {
                if (_filterChain.ApplyFilter(_filter, info.FullName))
                {
                    if (_progress != null)
                    {
                        _progress.Message = info.FullName;
                        _progress.Update();
                    }

                    BaseCompareObject o = folder.GetChild(info.Name); // Gets a child with the same name.
                    FileCompareObject fco = null;
                    bool conflict = false;

                    if (o == null) // If o is null, create a new file compare object
                        fco = new FileCompareObject(info.Name, numOfPaths, folder);
                    else
                    {
                        try
                        {
                            fco = (FileCompareObject)o; // Case o to a FileCompareObject is o is not null
                        }
                        catch (InvalidCastException) // If invalid cast, it means there is a FolderCompareObject with the exact same name.
                        {
                            _typeConflicts.Add(info.FullName); // Add to to conflicts
                            conflict = true;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CONFLICT, "Conflicted file detected " + info.FullName));
                        }
                    }

                    if (!conflict)
                    {
                        fco.CreationTimeUtc[index] = info.CreationTimeUtc.Ticks;
                        fco.LastWriteTimeUtc[index] = info.LastWriteTimeUtc.Ticks;
                        fco.Length[index] = info.Length;
                        fco.Exists[index] = true;

                        if (o == null)
                            folder.AddChild(fco); // Add the newly created FileCompareObject to this current folder
                    }
                }
            }
        }


        /// <summary>
        /// Processes and adds folders to the tree.
        /// </summary>
        /// <param name="folder"><see cref="FolderCompareObject"/> to process and add folders to.</param>
        /// <param name="numOfPaths">The total number of sync folders.</param>
        /// <param name="f">The <see cref="DirectoryInfo"/> to process and get folders from.</param>
        /// <param name="index">The index indicating which sync folder it belongs to.</param>
        private void ProcessFolders(FolderCompareObject folder, int numOfPaths, DirectoryInfo f, int index)
        {
            DirectoryInfo[] infos = f.GetDirectories();

            foreach (DirectoryInfo info in infos)
            {
                if (_filterChain.ApplyFilter(_filter, info.FullName))
                {
                    if (_progress != null)
                    {
                        _progress.Message = info.FullName;
                        _progress.Update();
                    }

                    BaseCompareObject o = folder.GetChild(info.Name); // Gets a child with the same name.
                    FolderCompareObject fco;
                    bool conflict = false;

                    if (o == null) // If o is null, create a new folder compare object.
                        fco = new FolderCompareObject(info.Name, numOfPaths, folder); // Create a new folder compare object
                    else
                    {
                        try
                        {
                            fco = (FolderCompareObject)o; // Cast o to a FolderCompareObject.
                        }
                        catch (InvalidCastException) // Happens when a file has the same name as the folder.
                        {
                            _typeConflicts.Add(info.FullName);
                            folder.RemoveChild(info.Name); //Remove file object
                            fco = new FolderCompareObject(info.Name, numOfPaths, folder); // Create a new folder compare object
                            conflict = true;
                            ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CONFLICT, "Conflicted file detected " + info.FullName));
                        }
                    }

                    fco.CreationTimeUtc[index] = info.CreationTimeUtc.Ticks;
                    fco.Exists[index] = true;

                    if (o == null || conflict)
                        folder.AddChild(fco); // Add the newly created FolderCompareObject to this current folder
                }
            }
        }

        #endregion

    }
}