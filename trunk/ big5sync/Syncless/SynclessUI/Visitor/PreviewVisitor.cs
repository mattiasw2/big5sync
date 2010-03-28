using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Exceptions;
using Syncless.CompareAndSync;
using Syncless.Core;
using System.Data;

namespace SynclessUI.Visitor
{
    public class PreviewVisitor : IVisitor
    {
        #region IVisitor Members
        private DataTable _syncData;

        public PreviewVisitor(DataTable syncData)
        {
            SyncData = syncData;
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
                    case MetaChangeType.New: operation = "Copy"; break;
                    case MetaChangeType.Delete: operation = "Delete"; break;
                    case MetaChangeType.Rename: operation = "Rename"; break;
                    case MetaChangeType.Update: operation = "Update"; break;
                }

                for (int i = 0; i < file.Priority.Length; i++)
                {
                    if (i != maxPriorityPos && file.Priority[i]!=file.Priority[maxPriorityPos])
                    {
                        var row = SyncData.NewRow();
                        row["Path1"] = Path.Combine(file.GetSmartParentPath(maxPriorityPos), file.Name);
                        row["Operation"] = operation;
                        row["Path2"] = Path.Combine(file.GetSmartParentPath(i), file.Name);
                        SyncData.Rows.Add(row);
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
                    case MetaChangeType.New: operation = "Copy"; break;
                    case MetaChangeType.Delete: operation = "Delete"; break;
                    case MetaChangeType.Rename: operation = "Rename"; break;
                }

                for (int i = 0; i < folder.Priority.Length; i++)
                {
                    if (i != maxPriorityPos && folder.Priority[i] != folder.Priority[maxPriorityPos])
                    {
                        var row = SyncData.NewRow();
                        row["Path1"] = Path.Combine(folder.GetSmartParentPath(maxPriorityPos), folder.Name);
                        row["Operation"] = operation;
                        row["Path2"] = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
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
