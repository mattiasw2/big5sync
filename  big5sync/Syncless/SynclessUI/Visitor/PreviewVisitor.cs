using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Exceptions;
using Syncless.CompareAndSync;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.Core;
using System.Data;

namespace SynclessUI.Visitor
{
    public class PreviewVisitor : IVisitor
    {
        #region IVisitor Members
        private DataTable _syncData;
        public const string Source = "source";
        public const string Dest = "destination";
        public const string Operation = "operation";

        private const string CopyConstant = "++";
        private const string DeleteConstant = "--";
        private const string UpdateConstant = "-=>";
        private const string RenameConstant = "--++";
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

            int maxPriorityPos = 0;
            for (int i = 0; i < numOfPaths; i++)
            {
                if (file.Priority[i] > file.Priority[maxPriorityPos])
                    maxPriorityPos = i;
            }
            
            
            if (file.Priority[maxPriorityPos] > 0)
            {
                string operation = "";
                switch (file.ChangeType[maxPriorityPos])
                {
                    case MetaChangeType.New: operation = CopyConstant; break;
                    case MetaChangeType.Delete: operation = DeleteConstant; break;
                    case MetaChangeType.Rename: operation = RenameConstant; break;
                    case MetaChangeType.Update: operation = UpdateConstant; break;
                    case MetaChangeType.NoChange: operation = CopyConstant; break;
                }

                for (int i = 0; i < file.Priority.Length; i++)
                {
                    if (i != maxPriorityPos && file.Priority[i]!=file.Priority[maxPriorityPos])
                    {
                        var row = SyncData.NewRow();
                        row[Source] = Path.Combine(file.GetSmartParentPath(maxPriorityPos), file.Name);
                        row[Operation] = operation;
                        row[Dest] = Path.Combine(file.GetSmartParentPath(i), file.Name);
                        SyncData.Rows.Add(row);
                        SyncData.AcceptChanges();

                    }
                }

            }

            //Basic logic: Look for highest priority and propagate it.

        }

        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            if (folder.Invalid)
                return;

            int maxPriorityPos = 0;
            for (int i = 0; i < numOfPaths; i++)
            {
                if (folder.Priority[i] > folder.Priority[maxPriorityPos])
                    maxPriorityPos = i;
            }


            if (folder.Priority[maxPriorityPos] > 0)
            {
                string operation = "";
                switch (folder.ChangeType[maxPriorityPos])
                {
                    case MetaChangeType.New: operation = CopyConstant; break;
                    case MetaChangeType.Delete: operation = DeleteConstant; break;
                    case MetaChangeType.Rename: operation = RenameConstant; break;
                }

                for (int i = 0; i < folder.Priority.Length; i++)
                {
                    if (i != maxPriorityPos && folder.Priority[i] != folder.Priority[maxPriorityPos])
                    {
                        var row = SyncData.NewRow();
                        row[Source] = Path.Combine(folder.GetSmartParentPath(maxPriorityPos), folder.Name);
                        row[Operation] = operation;
                        row[Dest] = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
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
