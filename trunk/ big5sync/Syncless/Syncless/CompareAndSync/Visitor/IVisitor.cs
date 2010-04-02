using Syncless.CompareAndSync.CompareObject;

namespace Syncless.CompareAndSync.Visitor
{
    public interface IVisitor
    {
        void Visit(FileCompareObject file, int numOfPaths);
        void Visit(FolderCompareObject folder, int numOfPaths);
        void Visit(RootCompareObject root);
    }
}
