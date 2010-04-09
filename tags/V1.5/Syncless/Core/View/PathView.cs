namespace Syncless.Core.View
{
    /// <summary>
    /// This is a View for TaggedPath
    /// </summary>
    public class PathView
    {
        /// <summary>
        /// Get the value if the path is available
        /// </summary>
        public bool IsAvailable { get; set; }
        /// <summary>
        /// Get the value if the path is deleted
        /// </summary>
        public bool IsDeleted { get; set; }
        /// <summary>
        /// Get and Set the Path
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Get the value if the path is missing
        /// </summary>
        public bool IsMissing { get; set; }
        /// <summary>
        /// Construct the PathView
        /// </summary>
        /// <param name="path">the path</param>
        public PathView(string path)
        {
            Path = path;
            IsAvailable = true;
            IsDeleted = false;
            IsMissing = false;
        }
    }
}
