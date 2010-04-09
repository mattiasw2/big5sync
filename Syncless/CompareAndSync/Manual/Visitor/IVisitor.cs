using Syncless.CompareAndSync.Manual.CompareObject;

namespace Syncless.CompareAndSync.Manual.Visitor
{
    /// <summary>
    /// Interface for all Visitors to implement.
    /// </summary>
    public interface IVisitor
    {
        /// <summary>
        /// Visit method for <see cref="FileCompareObject"/>.
        /// </summary>
        /// <param name="file">The <see cref="FileCompareObject"/> to process.</param>
        /// <param name="numOfPaths">The total number of folders to sync.</param>
        void Visit(FileCompareObject file, int numOfPaths);

        /// <summary>
        /// Visit method for <see cref="FolderCompareObject"/>
        /// </summary>
        /// <param name="folder">The <see cref="FolderCompareObject"/> to process.</param>
        /// <param name="numOfPaths">The total number of folders to sync.</param>
        void Visit(FolderCompareObject folder, int numOfPaths);

        /// <summary>
        /// Visit method for <see cref="RootCompareObject"/>.
        /// </summary>
        /// <param name="root">The <see cref="RootCompareObject"/> to process.</param>
        void Visit(RootCompareObject root);
    }
}