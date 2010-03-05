using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Filters.Exceptions;
using System.Diagnostics;
namespace Syncless.Filters
{
    public class FilterChain : AbstractFilter
    {
        private List<AbstractFilter> _filterList;

        public List<AbstractFilter> FilterList
        {
            get { return _filterList; }
        }
        public FilterChain()
        {
            _filterList = new List<AbstractFilter>();
        }
        
        public void AddFilter(AbstractFilter filter){
            if(filter is FilterChain){ // prevent loop
                throw new FilterChainException();
            }
            _filterList.Add(filter);
        }
        public override string Filter(string path)
        {
            Debug.Assert(path != null, "Path Cannot Be Null");
            foreach (AbstractFilter filter in _filterList)
            {
                path = filter.Filter(path);
                if (path == null)
                {
                    return null;
                }
            }
            return path;
        }

        public override List<string> Filter(List<string> paths)
        {
            Debug.Assert(paths != null, "Paths List cannot be Null");
            foreach (AbstractFilter filter in _filterList)
            {
                paths = filter.Filter(paths);
                if (paths.Count == 0)
                {
                    return paths;
                }
            }
            return paths;
        }
        public override bool Match(string path)
        {
            foreach (AbstractFilter filter in _filterList)
            {
                if (!filter.Match(path))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
