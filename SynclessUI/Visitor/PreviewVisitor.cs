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

namespace SynclessUI.Visitor
{
    public class PreviewVisitor : IVisitor
    {
        #region IVisitor Members

        private DataTable _syncData;
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

        private const string CopyArrow = "Icons\\new-sync-arrow.png";
        private const string DeleteArrow = "Icons\\delete-sync-arrow.png";
        private const string UpdateArrow = "Icons\\update-sync-arrow.png";
        private const string RenameArrow = "Icons\\rename-sync-arrow.png";

        private const string FileCopyToolTip = "File will be copied from Source to Destination";
        private const string FileUpdateToolTip = "File will be updated from Source to Destination";
        private const string FileDeleteToolTip = "File has been deleted from Source and will be in Destination";
        private const string FileRenameToolTip = "File has been renamed in Source and will be in Destination";

        private const string FolderCopyToolTip = "Folder will be copied from Source to Destination";
        private const string FolderUpdateToolTip = "Folder will be updated from Source to Destination";
        private const string FolderDeleteToolTip = "Folder has been deleted from Source and will be in Destination";
        private const string FolderRenameToolTip = "Folder has been renamed in Source and will be in Destination";
		
        private const string FolderIcon = "Icons\\folder.ico";
        private const string FileIcon = "Icons\\file.ico";

        public PreviewVisitor(DataTable syncData)
        {
            _syncData = syncData;
        }

        public DataTable SyncData
        {
            get { return _syncData; }
            set { _syncData = value; }
        }

        public void Visit(FileCompareObject fco, int numOfPaths)
        {
            if (fco.Invalid)
                return;

            int maxPriorityPos = fco.SourcePosition;

            if (fco.Priority[maxPriorityPos] > 0)
            {
                for (int i = 0; i < fco.Priority.Length; i++)
                {
                    if (i != maxPriorityPos && fco.Priority[i] != fco.Priority[maxPriorityPos])
                    {
                        string operation = string.Empty, source = string.Empty, dest = string.Empty, tooltip = string.Empty;
                        DataRow row = SyncData.NewRow();

                        switch (fco.ChangeType[maxPriorityPos])
                        {
                            case MetaChangeType.New:
                            case MetaChangeType.Update:
                            case MetaChangeType.NoChange:
                                source = Path.Combine(fco.GetSmartParentPath(maxPriorityPos), fco.Name);
                                dest = Path.Combine(fco.GetSmartParentPath(i), fco.Name);
                                operation = fco.Exists[i] ? UpdateArrow : CopyArrow;
                                tooltip = fco.Exists[i] ? FileUpdateToolTip : FileCopyToolTip;
                                break;
                            case MetaChangeType.Delete:
                                source = Path.Combine(fco.GetSmartParentPath(maxPriorityPos), fco.Name);
                                dest = Path.Combine(fco.GetSmartParentPath(i), fco.Name);
                                operation = DeleteArrow;
                                tooltip = FileDeleteToolTip;
                                break;
                            case MetaChangeType.Rename:
                                string oldName = Path.Combine(fco.GetSmartParentPath(i), fco.Name);
                                source = File.Exists(oldName) ? oldName : Path.Combine(fco.GetSmartParentPath(maxPriorityPos), fco.NewName);
                                dest = Path.Combine(fco.GetSmartParentPath(i), fco.NewName);
                                operation = File.Exists(oldName) ? RenameArrow : CopyArrow;
                                tooltip = File.Exists(oldName) ? FileRenameToolTip : FileCopyToolTip;
                                break;
                        }
                        row[Source] = source;
                        row[Operation] = operation;
                        row[Dest] = dest;
                        row[Tooltip] = tooltip;
                        row[SourceIcon] = FileIcon;
                        row[DestIcon] = FileIcon;
                        DateTime sourceDateTime = new DateTime(fco.LastWriteTimeUtc[maxPriorityPos]);
                        row[SourceLastModifiedDate] = sourceDateTime.ToShortDateString();
                        row[SourceLastModifiedTime] = sourceDateTime.ToShortTimeString();
                        row[SourceSize] = fco.Length[maxPriorityPos];

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

        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            if (folder.Invalid)
                return;

            int maxPriorityPos = folder.SourcePosition;

            if (folder.Priority[maxPriorityPos] > 0)
            {
                for (int i = 0; i < folder.Priority.Length; i++)
                {
                    if (i != maxPriorityPos && folder.Priority[i] != folder.Priority[maxPriorityPos])
                    {
                        string operation = string.Empty, source = string.Empty, dest = string.Empty, tooltip = string.Empty;

                        DataRow row = SyncData.NewRow();

                        switch (folder.ChangeType[maxPriorityPos])
                        {
                            case MetaChangeType.New:
                            case MetaChangeType.NoChange:
                            case MetaChangeType.Update:
                                source = Path.Combine(folder.GetSmartParentPath(maxPriorityPos), folder.Name);
                                dest = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
                                operation = CopyArrow;
                                tooltip = folder.Exists[i] ? FolderUpdateToolTip : FolderCopyToolTip;
                                break;
                            case MetaChangeType.Delete:
                                source = Path.Combine(folder.GetSmartParentPath(maxPriorityPos), folder.Name);
                                dest = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
                                operation = DeleteArrow;
                                tooltip = FolderDeleteToolTip;
                                break;
                            case MetaChangeType.Rename:
                                string oldFolderName = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
                                source = File.Exists(oldFolderName) ? oldFolderName : Path.Combine(folder.GetSmartParentPath(maxPriorityPos), folder.NewName);
                                dest = Path.Combine(folder.GetSmartParentPath(i), folder.NewName);
                                operation = Directory.Exists(oldFolderName) ? RenameArrow : CopyArrow;
                                tooltip = Directory.Exists(oldFolderName) ? FolderRenameToolTip : FolderCopyToolTip;
                                break;
                        }

                        row[Source] = source;
                        row[Dest] = dest;
                        row[Operation] = operation;
                        row[Tooltip] = tooltip;
                        row[SourceIcon] = FolderIcon;
                        row[DestIcon] = FolderIcon;
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
