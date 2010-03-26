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
            return new ExtensionFilter(pattern,mode);
        }
        public static Filter CreateConfigurationFilter()
        {
            return new SynclessConfigFilter();
        }
        public static Filter CreateArchiveFilter(string archiveName)
        {   
            return new SynclessArchiveFilter(archiveName);
        }
    }
}