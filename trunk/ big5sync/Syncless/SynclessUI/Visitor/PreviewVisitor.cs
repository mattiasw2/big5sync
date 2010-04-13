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

        private const string CopyConstant = "Icons\\green-sync-arrow.png";
        private const string DeleteConstant = "Icons\\red-sync-arrow.png";
        private const string UpdateConstant = "-=>";
        private const string RenameConstant = "Icons\\yellow-sync-arrow.png";

        private const string CopyToolTip = "File/Folder Will Be Copied from Source to Destination";
        private const string UpdateToolTip = "File/Folder Will Be Updated from Source to Destination";
        private const string DeleteToolTip = "File/Folder Has Been Deleted From Source and Will Be Likewise in Destination";
        private const string RenameToolTip = "File/Folder Has Been Renamed In Source and Will Be Likewise in Destination";

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
                        string operation = string.Empty,
                               source = string.Empty,
                               dest = string.Empty,
                               tooltip = string.Empty;

                        var row = SyncData.NewRow();

                        switch (fco.ChangeType[maxPriorityPos])
                        {
                            case MetaChangeType.New:
                            case MetaChangeType.Update:
                            case MetaChangeType.NoChange:
                                source = Path.Combine(fco.GetSmartParentPath(maxPriorityPos), fco.Name);
                                dest = Path.Combine(fco.GetSmartParentPath(i), fco.Name);
                                operation = fco.Exists[i] ? UpdateConstant : CopyConstant;
                                break;
                            case MetaChangeType.Delete:
                                source = Path.Combine(fco.GetSmartParentPath(maxPriorityPos), fco.Name);
                                dest = Path.Combine(fco.GetSmartParentPath(i), fco.Name);
                                operation = DeleteConstant;
                                tooltip = DeleteToolTip;
                                break;
                            case MetaChangeType.Rename:
                                string oldName = Path.Combine(fco.GetSmartParentPath(i), fco.Name);
                                source = File.Exists(oldName) ? oldName : Path.Combine(fco.GetSmartParentPath(maxPriorityPos), fco.NewName);
                                dest = Path.Combine(fco.GetSmartParentPath(i), fco.NewName);
                                operation = File.Exists(oldName) ? RenameConstant : CopyConstant;
                                tooltip = File.Exists(oldName) ? RenameToolTip : CopyToolTip;
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
                        } else
                        {
                            row[DestLastModifiedDate] = "-";
                            row[DestSize] = "-";
                        }


                        SyncData.Rows.Add(row);
                        //SyncData.AcceptChanges();
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

                        var row = SyncData.NewRow();

                        switch (folder.ChangeType[maxPriorityPos])
                        {
                            case MetaChangeType.New:
                            case MetaChangeType.NoChange:
                            case MetaChangeType.Update:
                                source = Path.Combine(folder.GetSmartParentPath(maxPriorityPos), folder.Name);
                                dest = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
                                operation = CopyConstant;
                                tooltip = CopyToolTip;
                                break;
                            case MetaChangeType.Delete:
                                source = Path.Combine(folder.GetSmartParentPath(maxPriorityPos), folder.Name);
                                dest = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
                                operation = DeleteConstant;
                                tooltip = DeleteToolTip;
                                break;
                            case MetaChangeType.Rename:
                                string oldFolderName = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
                                source = File.Exists(oldFolderName) ? oldFolderName : Path.Combine(folder.GetSmartParentPath(maxPriorityPos), folder.NewName);
                                dest = Path.Combine(folder.GetSmartParentPath(i), folder.NewName);
                                operation = Directory.Exists(oldFolderName) ? RenameConstant : CopyConstant;
                                tooltip = Directory.Exists(oldFolderName) ? RenameToolTip : CopyToolTip;
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
