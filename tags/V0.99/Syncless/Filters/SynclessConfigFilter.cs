using System;

namespace Syncless.Filters
{
    public class SynclessConfigFilter : Filter
    {
        private const string SYNCLESS = ".syncless";

        public SynclessConfigFilter()
            : base(FilterMode.EXCLUDE)
        {
        }

        public override bool Match(string path)
        {
            string[] tokens = path.Split(new char[]{'\\'});
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
