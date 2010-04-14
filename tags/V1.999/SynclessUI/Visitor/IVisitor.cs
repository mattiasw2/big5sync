/*
 * 
 * Author: Steve Teo Wai Ming
 * 
 */

using Syncless.CompareAndSync.Manual.CompareObject;

namespace SynclessUI.Visitor
{
    public interface IVisitor
    {
        void Visit(FileCompareObject file, int numOfPaths);
        void Visit(FolderCompareObject folder, int numOfPaths);
        void Visit(RootCompareObject root);
    }
}
