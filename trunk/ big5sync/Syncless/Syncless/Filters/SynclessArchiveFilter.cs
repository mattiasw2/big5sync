/*
 * 
 * Author: Eric Ng Jun Feng
 * 
 */
namespace Syncless.Filters
{
    /// <summary>
    /// Filter that filter the Syncless Archive and Conflict Filter
    /// </summary>
    public class SynclessArchiveFilter : Filter
    {
        private readonly string _archiveName;
        /// <summary>
        /// Initialize a Syncless Archive Filter
        /// </summary>
        /// <param name="archiveName"></param>
        public SynclessArchiveFilter(string archiveName)
            : base(FilterMode.EXCLUDE)
        {
            _archiveName = archiveName;
        }
        /// <summary>
        /// override the Match method
        /// </summary>
        /// <param name="path">The path to match.</param>
        /// <returns>true if it matches</returns>
        public override bool Match(string path)
        {
            string[] tokens = path.Split(new char[]{'\\'});
            foreach (string token in tokens)
            {
                if (token.ToLower().Equals(_archiveName.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
