using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Syncless.Core
{
    internal enum TableType
    {
        Create, Update, Rename, Delete
    }
    internal class PathTable
    {
        private List<PathPair> _createEventPathPair;
        private List<PathPair> _updateEventPathPair;
        private List<PathPair> _renameEventPathPair;


        public PathTable()
        {
            _createEventPathPair = new List<PathPair>();
            _updateEventPathPair = new List<PathPair>();
            _renameEventPathPair = new List<PathPair>();
        }

        public PathPair RemovePathPair(string source, string dest, TableType type)
        {
            PathPair returnPair = null;
            List<PathPair> usedTable = null;
            switch (type)
            {
                case TableType.Create: usedTable = _createEventPathPair; break;
                case TableType.Update: usedTable = _updateEventPathPair; break;
                case TableType.Rename: usedTable = _renameEventPathPair; break;


            }
            if (usedTable == null)
            {
                return null;
            }
            lock (usedTable)
            {
                PathPair pathPair = new PathPair(source, dest);
                foreach (PathPair pair in usedTable)
                {
                    if (pathPair.Equals(pair))
                    {
                        returnPair = pair;
                    }
                }
                usedTable.Remove(returnPair);
            }

            return returnPair;
        }
        public bool JustPop(string source, string dest, TableType type)
        {
            PathPair returnPair = null;
            List<PathPair> usedTable = null;
            switch (type)
            {
                case TableType.Create: usedTable = _createEventPathPair; break;
                case TableType.Update: usedTable = _updateEventPathPair; break;
                case TableType.Rename: usedTable = _renameEventPathPair; break;


            }
            if (usedTable == null)
            {
                return false;
            }
            lock (usedTable)
            {
                PathPair pathPair = new PathPair(source, dest);
                foreach (PathPair pair in usedTable)
                {
                    if (pathPair.Equals(pair))
                    {
                        returnPair = pair;
                    }
                }
                usedTable.Remove(returnPair);
            }

            return returnPair != null;
        }
        public bool AddPathPair(string source, string dest, TableType type)
        {
            PathPair returnPair = null;
            List<PathPair> usedTable = null;
            switch (type)
            {
                case TableType.Create: usedTable = _createEventPathPair; break;
                case TableType.Update: usedTable = _updateEventPathPair; break;
                case TableType.Rename: usedTable = _renameEventPathPair; break;


            }
            if (usedTable == null)
            {
                return false;
            }
            lock (usedTable)
            {
                PathPair pathPair = new PathPair(source, dest);
                if (!Contains(pathPair, type))
                {
                    usedTable.Add(pathPair);
                    return true;
                }

            }
            return false;
        }

        public bool Contains(PathPair pair, TableType type)
        {
            List<PathPair> usedTable = null;
            switch (type)
            {
                case TableType.Create: usedTable = _createEventPathPair; break;
                case TableType.Update: usedTable = _updateEventPathPair; break;
                case TableType.Rename: usedTable = _renameEventPathPair; break;
            }
            if (usedTable == null)
            {
                return false;
            }
            return usedTable.Contains(pair);
        }

        public int Count
        {
            get { return _createEventPathPair.Count + _updateEventPathPair.Count + _renameEventPathPair.Count; }

        }

    }
    internal class PathPair
    {
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

        public bool Equals(PathPair pair)
        {
            if (Dest != pair.Dest) return false;
            if (Source != pair.Source) return false;
            return true;
        }
        public PathPair(string source, string dest)
        {
            Source = source;
            Dest = dest;
        }
    }

    internal class PathTableReader
    {
        private PathTable _table;
        private Thread _reader;
        public PathTableReader(PathTable table)
        {
            _table = table;
        }
        public void Start()
        {
            _reader = new Thread(Run);
            _reader.Start();
        }
        public void Stop()
        {
            _reader.Abort();
        }
        private void Run()
        {
            while (true)
            {
                Console.WriteLine("Entry Count :" + _table.Count);
                Thread.Sleep(5000);
            }
        }

    }
}
