namespace Syncless.Filters
{
    /// <summary>
    /// Factory class for Creating Filter.
    /// </summary>
    public class FilterFactory
    {
        /// <summary>
        /// Reference to <see cref="FilterMode.INCLUDE"/>
        /// </summary>
        public const FilterMode INCLUDE = FilterMode.INCLUDE;
        /// <summary>
        /// Reference to <see cref="FilterMode.EXCLUDE"/>
        /// </summary>
        public const FilterMode EXCLUDE = FilterMode.EXCLUDE;
        /// <summary>
        /// Factory Method to create Extension Filter
        /// </summary>
        /// <param name="pattern">the pattern of extension filter</param>
        /// <param name="mode">the filter mode. See <see cref="FilterMode"/></param>
        /// <returns>The Filter object.</returns>
        public static Filter CreateExtensionFilter(string pattern, FilterMode mode)
        {   
            return new ExtensionFilter(pattern,mode);
        }
        /// <summary>
        /// Factory Method to create Configuration Filter
        /// </summary>
        /// <returns>The Filter object</returns>
        public static Filter CreateConfigurationFilter()
        {
            return new SynclessConfigFilter();
        }
        /// <summary>
        /// Factory Method to create Archive Filter
        /// </summary>
        /// <param name="archiveName">Name of the archive</param>
        /// <returns>The Filter object</returns>
        public static Filter CreateArchiveFilter(string archiveName)
        {   
            return new SynclessArchiveFilter(archiveName);
        }
    }
}