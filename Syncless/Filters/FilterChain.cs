using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
namespace Syncless.Filters
{
    public class FilterChain
    {
        /// <summary>
        /// Apply Filter , Include then Exclude.
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="?"></param>
        public virtual List<string> ApplyFilter(List<Filter> filters , List<string> patterns){

            List<string> outputList = new SynclessConfigFilter().ApplyFilter(patterns);
            if (filters == null)
            {
                return outputList;
            }
            List<Filter>[] filtersList = Split(filters);
            outputList = ApplyInclude(filtersList[0], outputList);
            outputList = ApplyExclude(filtersList[1], outputList);

            return outputList;
        }
        /// <summary>
        /// Check if a Path pass thru the filter
        /// </summary>
        /// <param name="filters">the list of filters</param>
        /// <param name="path">the path to filter</param>
        /// <returns></returns>
        public virtual bool ApplyFilter(List<Filter> filters, string path)
        {
            List<string> temp = new List<string>();
            temp.Add(path);
            List<string> outputList = ApplyFilter(filters, temp);
            return outputList.Count>0;
        }

        #region private methods
        private List<Filter>[] Split(List<Filter> filters)
        {
            List<Filter>[] filterList = new List<Filter>[2];
            filterList[0] = new List<Filter>();
            filterList[1] = new List<Filter>();
            foreach (Filter filter in filters)
            {
                if (filter.Mode == FilterMode.INCLUDE)
                {
                    filterList[0].Add(filter);
                }
                else
                {
                    filterList[1].Add(filter);
                }
            }
            return filterList;
        }
        private List<string> ApplyInclude(List<Filter> filters, List<string> patterns)
        {
            if (filters.Count == 0 || patterns.Count==0)
            {
                //if no include filter, default include all
                return patterns;
            }
            // all files that pass through the filter should be included.
            // THis is A OR Operation
            List<string> outputList = new List<string>();
            foreach (Filter  filter in filters)
            {
                Debug.Assert(filter.Mode == FilterMode.INCLUDE);
                List<string> tempList = filter.ApplyFilter(patterns);
                if (tempList.Count == 0)
                {
                    continue;
                }
                else
                {
                    outputList = UnionString(outputList, tempList);
                }
            }
            return outputList;
        }
        private List<string> ApplyExclude(List<Filter> filters, List<string> patterns)
        {
            // All files that pass the criteria must be excluded
            // THis is a OR also.
            List<string> outputList = new List<string>();
            outputList.AddRange(patterns);
            foreach (Filter filter in filters)
            {
                Debug.Assert(filter.Mode == FilterMode.EXCLUDE);
                outputList = filter.ApplyFilter(patterns);
                if (outputList.Count == 0)
                {
                    break;
                }
            }
            return outputList;
        }
        private List<string> UnionString(List<string> list1, List<string> list2)
        {
            List<string> returnedList = new List<string>();
            returnedList.AddRange(list1);
            foreach (string item in list2)
            {
                if (!returnedList.Contains(item))
                {
                    returnedList.Add(item);
                }
            }
            return returnedList;
        }
        #endregion
    }
}
