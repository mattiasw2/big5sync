﻿/*
 * 
 * Author: Soh Yuan Chin
 * 
 */

using System;
using System.IO;
using Syncless.CompareAndSync.Exceptions;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.Core;
using Syncless.Logging;

namespace Syncless.CompareAndSync.Manual.Visitor
{
    /// <summary>
    /// ConflictVisitor is responsible for handling conflicted types (when file and folder have the same name, including extension).
    /// </summary>
    public class ConflictVisitor : IVisitor
    {
        private readonly SyncConfig _syncConfig;

        /// <summary>
        /// Instantiates an instance of <c>ConflictVisitor</c> with a sync configuration.
        /// </summary>
        /// <param name="syncConfig">The sync configuration to pass in.</param>
        public ConflictVisitor(SyncConfig syncConfig)
        {
            _syncConfig = syncConfig;
        }

        /// <summary>
        /// Visit implementation for <see cref="FileCompareObject"/>.
        /// </summary>
        /// <param name="file">The <see cref="FileCompareObject"/> to process.</param>
        /// <param name="numOfPaths"></param>
        public void Visit(FileCompareObject file, int numOfPaths)
        {
            if (file.ConflictPositions == null || file.ConflictPositions.Count == 0)
                return;

            foreach (int i in file.ConflictPositions)
                ConflictHandler(file, i);
        }

        // Do nothing for folder
        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
        }

        // Do nothing for root
        public void Visit(RootCompareObject root)
        {
        }

        private void ConflictHandler(FileCompareObject fco, int fileIndex)
        {
            string src = Path.Combine(fco.GetSmartParentPath(fileIndex), fco.Name);
            string conflictFolder = Path.Combine(fco.GetSmartParentPath(fileIndex), _syncConfig.ConflictDir);
           
            if (!Directory.Exists(conflictFolder))
                Directory.CreateDirectory(conflictFolder);

            string currTime = String.Format("{0:MMddHHmmss}", DateTime.Now) + "_";
            string dest = Path.Combine(conflictFolder, currTime + fco.Name);

            try
            {
                CommonMethods.CopyFile(src, dest); // Copy conflicted file from to the conflict folder
                CommonMethods.DeleteFile(src); // Delete conflicted file
            }
            catch (CopyFileException)
            {
                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error copying file from " + src + " to " + dest));
            }
            catch (DeleteFileException)
            {
                ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_ERROR, "Error deleting file " + src));
            }
        }
    }
}