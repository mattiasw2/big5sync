using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Filters
{
    public class SynclessArchiveFilter : Filter
    {
        private string _archiveName;

        public SynclessArchiveFilter(string archiveName)
            : base(FilterMode.EXCLUDE)
        {
            _archiveName = archiveName;
        }

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
