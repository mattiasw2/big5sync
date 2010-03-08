using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CompareAndSync.CompareObject;

namespace CompareAndSync.Visitor
{
    public class ComparerVisitor : IVisitor
    {
        #region IVisitor Members

        public void Visit(FileCompareObject file, int level, string[] currentPaths)
        {
            int mostUpdatedPos = 0;

            for (int i = 0; i < currentPaths.Length; i++)
            {
                if (file.ExistsArray[i])
                {
                    mostUpdatedPos = i;
                    break;
                }
            }

            for (int i = mostUpdatedPos + 1; i < currentPaths.Length - 1; i++)
            {
                if (!file.ExistsArray[i])
                    continue;

                if (file.Length[mostUpdatedPos] != file.Length[i] || file.Hash[mostUpdatedPos] != file.Hash[i])
                {
                    if (file.LastWriteTime[i] > file.LastWriteTime[mostUpdatedPos])
                    {
                        mostUpdatedPos = i;
                    }
                    else if (file.LastWriteTime[i] == file.Length[mostUpdatedPos])
                    {
                        //Handle conflicts here?
                    }
                }
            }

            file.Priority[mostUpdatedPos]++;
        }

        public void Visit(FolderCompareObject folder, int level, string[] currentPaths)
        {
            throw new NotImplementedException();
        }

        public void Visit(RootCompareObject root)
        {
            // Do nothing
        }

        #endregion
    }
}
