using Syncless.CompareAndSync.CompareObject;
using Syncless.CompareAndSync.Enum;

namespace Syncless.CompareAndSync.Visitor
{
    public class IgnoreMetaDataVisitor : IVisitor
    {
        #region IVisitor Members

        public void Visit(FileCompareObject file, int numOfFiles)
        {
            for (int i = 0; i < numOfFiles; i++)
                file.ChangeType[i] = MetaChangeType.New;
        }

        public void Visit(FolderCompareObject folder, int numOfFiles)
        {
            for (int i = 0; i < numOfFiles; i++)
                folder.ChangeType[i] = MetaChangeType.New;
        }

        public void Visit(RootCompareObject root) { }

        #endregion
    }
}
