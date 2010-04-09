namespace Syncless.Core
{
    /// <summary>
    /// All User Interface interacting with Logic Layer is required to implement this interface.
    /// </summary>
    public interface IUIInterface
    {
        /// <summary>
        /// Get the Application Path
        /// </summary>
        /// <returns>the folder containing the Syncless.exe</returns>
        string getAppPath();
        /// <summary>
        /// Provided for Logic Layer to inform the UI that a Drive have changed.
        /// </summary>
        void DriveChanged();
        /// <summary>
        /// Provided for Logic layer to inform the UI that the tag with the particular tagname have been changed
        /// </summary>
        /// <param name="tagName">The name of the tag that have been changed</param>
        void TagChanged(string tagName);
        /// <summary>
        /// Provided for Logic layer to inform the UI of changes in some of the tags
        /// </summary>
        void TagsChanged();
        /// <summary>
        /// Provided for Logic layer to inform the UI of changes in the Tagged Path.
        /// </summary>
        void PathChanged();
    }
}
