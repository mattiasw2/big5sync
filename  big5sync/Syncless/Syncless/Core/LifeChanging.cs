using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Syncless.Core
{
    internal class LifeChanging
    {
        private List<PathPair> _pathList;
        private Object _lock;
        public LifeChanging()
        {
            _pathList = new List<PathPair>();
            _lock = new object();
        }
        public List<PathPair> GetPairBySource(string source)
        {
            List<PathPair> returnList = new List<PathPair>();
            lock (_lock)
            {
                foreach (PathPair pair in _pathList)
                {
                    if (pair.Dest.ToLower().Equals(source.ToLower()))
                    {
                        returnList.Add(pair);
                    }
                }
            }
            return returnList;
        }
        public List<PathPair> GetPairByDestination(string source)
        {
            List<PathPair> returnList = new List<PathPair>();
            lock (_lock)
            {
                foreach (PathPair pair in _pathList)
                {
                    if (pair.Source.ToLower().Equals(source.ToLower()))
                    {
                        returnList.Add(pair);
                    }
                }
            }
            return returnList;
        }
        /// <summary>
        /// Will Remove the Pair and return
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public PathPair RemovePair(string source, string destination)
        {
            PathPair newPair = new PathPair(source,destination);
            lock (_lock)
            {
                foreach (PathPair pair in _pathList)
                {
                    if (pair.Equals(newPair))
                    {
                        return pair;
                    }
                }

            }
            return null;
        }
    }

    internal class PathPair
    {
        public PathPair(string src , string dest)
        {
            Source = src;
            Dest = dest;
        }

        public string Source
        {
            get;
            set;
        }
        public string Dest
        {
            get;
            set;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(PathPair other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Source.ToLower(), Source.ToLower()) && Equals(other.Dest.ToLower(), Dest.ToLower());
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Source != null ? Source.GetHashCode() : 0)*397) ^ (Dest != null ? Dest.GetHashCode() : 0);
            }
        }
    }
}
