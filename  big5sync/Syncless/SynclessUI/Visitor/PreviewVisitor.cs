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

        public void Visit(FileCompareObject file, int numOfPaths)
        {
            if (file.Invalid)
                return;

            int maxPriorityPos = file.SourcePosition;

            if (file.Priority[maxPriorityPos] > 0)
            {
                for (int i = 0; i < file.Priority.Length; i++)
                {
                    if (i != maxPriorityPos && file.Priority[i] != file.Priority[maxPriorityPos])
                    {
                        string operation = string.Empty,
                               source = string.Empty,
                               dest = string.Empty;
                        Console.WriteLine(operation);
                        var row = SyncData.NewRow();

                        switch (file.ChangeType[maxPriorityPos])
                        {
                            case MetaChangeType.New:
                            case MetaChangeType.Update:
                            case MetaChangeType.NoChange:
                                source = Path.Combine(file.GetSmartParentPath(maxPriorityPos), file.Name);
                                dest = Path.Combine(file.GetSmartParentPath(i), file.Name);
                                operation = file.Exists[i] ? UpdateConstant : CopyConstant;
                                break;
                            case MetaChangeType.Delete:
                                source = Path.Combine(file.GetSmartParentPath(maxPriorityPos), file.Name);
                                dest = Path.Combine(file.GetSmartParentPath(i), file.Name);
                                operation = DeleteConstant;
                                break;
                            case MetaChangeType.Rename:
                                string oldName = Path.Combine(file.GetSmartParentPath(i), file.Name);
                                source = File.Exists(oldName) ? oldName : Path.Combine(file.GetSmartParentPath(maxPriorityPos), file.NewName);
                                dest = Path.Combine(file.GetSmartParentPath(i), file.NewName);
                                operation = File.Exists(oldName) ? RenameConstant : CopyConstant;
                                break;
                        }
                        row[Source] = source;
                        row[Operation] = operation;
                        row[Dest] = dest;
                        SyncData.Rows.Add(row);
                        SyncData.AcceptChanges();
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
