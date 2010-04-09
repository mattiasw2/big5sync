using System;

namespace Syncless.Filters
{
    /// <summary>
    /// The Filter for Syncless Config Filter (.syncless folder)
    /// </summary>
    public class SynclessConfigFilter : Filter
    {
        /// <summary>
        /// Constant for the folder.
        /// </summary>
        private const string SYNCLESS = ".syncless";

        /// <summary>
        /// Initialize the Syncless Config Filter
        /// </summary>
        public SynclessConfigFilter()
            : base(FilterMode.EXCLUDE)
        {
        }
        /// <summary>
        /// Override the match method.
        /// </summary>
        /// <param name="path">The path to match</param>
        /// <returns>true if it match, otherwise false</returns>
        public override bool Match(string path)
        {
            string[] tokens = path.Split(new []{'\\'});
            foreach (string token in tokens)
            {
                if (token.ToLower().Equals(SYNCLESS))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
