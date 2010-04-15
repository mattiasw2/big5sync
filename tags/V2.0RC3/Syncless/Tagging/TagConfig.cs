/*
 * 
 * Author: Goh Khoon Hiang
 * 
 */

namespace Syncless.Tagging
{
    /// <summary>
    /// TagConfig class provides the configuration options of <see cref="Tag">Tag</see> object
    /// </summary>
    internal class TagConfig
    {
        private bool _isSeamless;

        /// <summary>
        /// Gets or sets the _isSeamless attribute of a <see cref="Tag">Tag</see> object
        /// </summary>
        public bool IsSeamless
        {
            get { return _isSeamless; }
            set { _isSeamless = value; }
        }
        
        /// <summary>
        /// Creates a new TagConfig object
        /// </summary>
        /// <remarks>The IsSeamless property is by default False</remarks>
        public TagConfig()
        {
            _isSeamless = false;
        }
    }
}
