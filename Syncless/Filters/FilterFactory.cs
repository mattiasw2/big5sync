using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Filters
{
    public class FilterFactory
    {
        public const FilterMode INCLUDE = FilterMode.INCLUDE;
        public const FilterMode EXCLUDE = FilterMode.EXCLUDE;

        public static Filter CreateExtensionFilter(string pattern, FilterMode mode)
        {
            ExtensionFilter filter = new ExtensionFilter(pattern,mode);
            return filter;
        }
        public static Filter CreateConfigurationFilter()
        {
            return new SynclessConfigFilter();
        }
        public static Filter CreateArchiveFilter(string archiveName)
        {
            SynclessArchiveFilter filter = new SynclessArchiveFilter(archiveName);
            return filter;
        }
    }
}