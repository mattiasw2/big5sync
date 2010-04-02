using System;
using System.IO;
using Syncless.CompareAndSync.CompareObject;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Exceptions;
using Syncless.Core;
using Syncless.Logging;

namespace Syncless.CompareAndSync.Visitor
{
    public class HashVisitor : IVisitor
    {
        #region IVisitor Members

        public void Visit(FileCompareObject file, int numOfPaths)
        {
            for (int i = 0; i < numOfPaths; i++)
            {
                if (file.CreationTime[i] == file.MetaCreationTime[i] && file.LastWriteTime[i] == file.MetaLastWriteTime[i] && file.Length[i] == file.MetaLength[i])
                    file.Hash[i] = file.MetaHash[i];
                else
                {
                    try
                    {
                        file.Hash[i] = CommonMethods.CalculateMD5Hash(Path.Combine(file.GetSmartParentPath(i), file.Name));
                    }
                    catch (HashFileException)
                    {
                        ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(
                            new LogData(LogEventType.FSCHANGE_ERROR,
                                        "Error hashing " +
                                        Path.Combine(file.GetSmartParentPath(i), file.Name + ".")));
                        file.FinalState[i] = FinalState.Error;
                        file.Invalid = true; //EXP
                    }
                }
            }
        }

        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            //Do nothing
        }

        public void Visit(RootCompareObject root)
        {
            //Do nothing
        }

        #endregion
    }
}
