/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using System;
using System.Data;
using System.IO;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.CompareAndSync.Manual.Visitor;

namespace SynclessUI.Visitor
{
    /// <summary>
    /// This visitor is the equivalent of the Visitor Pattern Logic that is present in SyncerVisitor, except that instead of
    /// visiting each FileCompareObject/FolderCompareObject to decide what to sync, it reads the logic and
    /// populates the DataTable to be used by the DataGrid.
    /// </summary>
    public class PreviewVisitor : IVisitor
    {
        #region IVisitor Members

        private DataTable _syncData;

        // Constants for all the datagrid columns, which is used in PreviewVisitor and PreviewSyncWindow
        public const string Source = "source";
        public const string Dest = "destination";
        public const string Operation = "operation";
        public const string Tooltip = "tooltip";
        public const string SourceIcon = "sourceicon";
        public const string DestIcon = "desticon";
        public const string SourceLastModifiedDate = "sourcelastmodifieddate";
        public const string SourceLastModifiedTime = "sourcelastmodifiedtime";
        public const string SourceSize = "sourcesize";
        public const string DestLastModifiedDate = "destlastmodifieddate";
        public const string DestLastModifiedTime = "destlastmodifiedtime";
        public const string DestSize = "destsize";

        //Path for each of the SyncOperations 
        private const string CopyArrow = "Icons\\new-sync-arrow.png";
        private const string DeleteArrow = "Icons\\delete-sync-arrow.png";
        private const string UpdateArrow = "Icons\\update-sync-arrow.png";
        private const string RenameArrow = "Icons\\rename-sync-arrow.png";

        // Tooltips for each of the file sync operations
        private const string FileCopyToolTip = "File will be copied from Source to Destination";
        private const string FileUpdateToolTip = "File will be updated from Source to Destination";
        private const string FileDeleteToolTip = "File has been deleted from Source and will be in Destination";
        private const string FileRenameToolTip = "File has been renamed in Source and will be in Destination";

        // Tooltips for each of the folder sync operations
        private const string FolderCopyToolTip = "Folder will be copied from Source to Destination";
        private const string FolderUpdateToolTip = "Folder will be updated from Source to Destination";
        private const string FolderDeleteToolTip = "Folder has been deleted from Source and will be in Destination";
        private const string FolderRenameToolTip = "Folder has been renamed in Source and will be in Destination";
		
        // Path to the File/FolderIcon for use in image
        private const string FolderIcon = "Icons\\folder.ico";
        private const string FileIcon = "Icons\\file.ico";

        /// <summary>
        /// Initializes the PreviewVisitor
        /// </summary>
        /// <param name="syncData">DataTable to be populated by the visit operations in PreviewVisitor</param>
        public PreviewVisitor(DataTable syncData)
        {
            _syncData = syncData;
        }

        /// <summary>
        /// Property for the DataTable _SyncData
        /// </summary>
        public DataTable SyncData
        {
            get { return _syncData; }
            set { _syncData = value; }
        }

        /// <summary>
        /// For every FileCompareObject, check which path has the max priority and then decide
        /// what sync operation should be approriate for each FileCompareObject. Once the path of the max
        /// priority has been set, for every other path which is not the max priority, populate the datagrid
        /// with by adding a DataRow for each operation that needs to be done.
        /// </summary>
        /// <param name="fco">FileCompareObject to be visited</param>
        /// <param name="numOfPaths">Total number of paths present in the particular FileCompareObject</param>
        public void Visit(FileCompareObject fco, int numOfPaths)
        {
            if (fco.Invalid)
                return;

            // find max priortiy position
            int maxPriorityPos = fco.SourcePosition;

            if (fco.Priority[maxPriorityPos] > 0)
            {
                for (int i = 0; i < numOfPaths; i++)
                {
                    if (i != maxPriorityPos && fco.Priority[i] != fco.Priority[maxPriorityPos])
                    {
                        string operation = string.Empty, source = string.Empty, dest = string.Empty, tooltip = string.Empty;
                        DataRow row = SyncData.NewRow();

                        switch (fco.ChangeType[maxPriorityPos])
                        {
                            // if the MetaChangeType is of New, Update, Change
                            case MetaChangeType.New:
                            case MetaChangeType.Update:
                            case MetaChangeType.NoChange:
                                source = Path.Combine(fco.GetSmartParentPath(maxPriorityPos), fco.Name);
                                dest = Path.Combine(fco.GetSmartParentPath(i), fco.Name);
                                operation = fco.Exists[i] ? UpdateArrow : CopyArrow;
                                tooltip = fco.Exists[i] ? FileUpdateToolTip : FileCopyToolTip;
                                break;
                            // if the MetaChangeType is of Delete type
                            case MetaChangeType.Delete:
                                source = Path.Combine(fco.GetSmartParentPath(maxPriorityPos), fco.Name);
                                dest = Path.Combine(fco.GetSmartParentPath(i), fco.Name);
                                operation = DeleteArrow;
                                tooltip = FileDeleteToolTip;
                                break;
                            // if the MetaChangeType is of Rename type
                            case MetaChangeType.Rename:
                                string oldName = Path.Combine(fco.GetSmartParentPath(i), fco.Name);
                                source = File.Exists(oldName) ? oldName : Path.Combine(fco.GetSmartParentPath(maxPriorityPos), fco.NewName);
                                dest = Path.Combine(fco.GetSmartParentPath(i), fco.NewName);
                                operation = File.Exists(oldName) ? RenameArrow : CopyArrow;
                                tooltip = File.Exists(oldName) ? FileRenameToolTip : FileCopyToolTip;
                                break;
                        }
                        
                        // set all properties of the DataRow which has been decided by the switch case above

                        row[Source] = source;
                        row[Operation] = operation;
                        row[Dest] = dest;
                        row[Tooltip] = tooltip;
                        row[SourceIcon] = FileIcon;
                        row[DestIcon] = FileIcon;
                        
                        // Set Date/Time/Length of FCO.
                        DateTime sourceDateTime = new DateTime(fco.LastWriteTimeUtc[maxPriorityPos]);
                        row[SourceLastModifiedDate] = sourceDateTime.ToShortDateString();
                        row[SourceLastModifiedTime] = sourceDateTime.ToShortTimeString();
                        row[SourceSize] = fco.Length[maxPriorityPos];

                        // Check if a particular file path exists
                        if (fco.Exists[i])
                        {
                            DateTime destDateTime = new DateTime(fco.LastWriteTimeUtc[i]);
                            row[DestLastModifiedDate] = destDateTime.ToShortDateString();
                            row[DestLastModifiedTime] = destDateTime.ToShortTimeString();
                            row[DestSize] = fco.Length[i];
                        }
                        else
                        {
                            row[DestLastModifiedDate] = "-";
                            row[DestSize] = "-";
                        }

                        SyncData.Rows.Add(row);
                    }
                }

            }
        }

        /// <summary>
        /// For every FolderCompareObject, check which path has the max priority and then decide
        /// what sync operation should be approriate for each FolderCompareObject. Once the path of the max
        /// priority has been set, for every other path which is not the max priority, populate the datagrid
        /// with by adding a DataRow for each operation that needs to be done.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="numOfPaths"></param>
        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            if (folder.Invalid)
                return;

            int maxPriorityPos = folder.SourcePosition;

            if (folder.Priority[maxPriorityPos] > 0)
            {
                for (int i = 0; i < numOfPaths; i++)
                {
                    if (i != maxPriorityPos && folder.Priority[i] != folder.Priority[maxPriorityPos])
                    {
                        string operation = string.Empty, source = string.Empty, dest = string.Empty, tooltip = string.Empty;

                        DataRow row = SyncData.NewRow();

                        switch (folder.ChangeType[maxPriorityPos])
                        {
                            // if the MetaChangeType is of New, Update, Change
                            case MetaChangeType.New:
                            case MetaChangeType.NoChange:
                            case MetaChangeType.Update:
                                source = Path.Combine(folder.GetSmartParentPath(maxPriorityPos), folder.Name);
                                dest = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
                                operation = CopyArrow;
                                tooltip = folder.Exists[i] ? FolderUpdateToolTip : FolderCopyToolTip;
                                break;
                            // if the MetaChangeType is of Delete type
                            case MetaChangeType.Delete:
                                source = Path.Combine(folder.GetSmartParentPath(maxPriorityPos), folder.Name);
                                dest = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
                                operation = DeleteArrow;
                                tooltip = FolderDeleteToolTip;
                                break;
                            // if the MetaChangeType is of Rename type
                            case MetaChangeType.Rename:
                                string oldFolderName = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
                                source = File.Exists(oldFolderName) ? oldFolderName : Path.Combine(folder.GetSmartParentPath(maxPriorityPos), folder.NewName);
                                dest = Path.Combine(folder.GetSmartParentPath(i), folder.NewName);
                                operation = Directory.Exists(oldFolderName) ? RenameArrow : CopyArrow;
                                tooltip = Directory.Exists(oldFolderName) ? FolderRenameToolTip : FolderCopyToolTip;
                                break;
                        }

                        // set all properties of the DataRow which has been decided by the switch case above
                        row[Source] = source;
                        row[Dest] = dest;
                        row[Operation] = operation;
                        row[Tooltip] = tooltip;
                        row[SourceIcon] = FolderIcon;
                        row[DestIcon] = FolderIcon;

                        // Set size and date/time for Folder to none
                        row[SourceLastModifiedDate] = "-";
                        row[SourceSize] = "-";
                        row[DestLastModifiedDate] = "-";
                        row[DestSize] = "-";
                        _syncData.Rows.Add(row);
                    }
                }

            }
        }

        public void Visit(RootCompareObject root)
        {
            //Do nothing
        }

        #endregion

    }


}
