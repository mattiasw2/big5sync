using System.Collections.Generic;

namespace Syncless.Core
{
    /// <summary>
    /// The enum for the 4 different type of path table
    /// </summary>
    internal enum TableType
    {
        Create, Update, Rename, Delete
    }
    /// <summary>
    /// PathTable contains the expected event during the handling of seamless synchronization.
    /// This is used to reduce the number of redundant request that is send to CompareAndSyncController.
    /// </summary>
    internal class PathTable
    {
        /// <summary>
        /// For Create Event
        /// </summary>
        private readonly List<PathPair> _createEventPathPair;
        /// <summary>
        /// For Update Event
        /// </summary>
        private readonly List<PathPair> _updateEventPathPair;
        /// <summary>
        /// For Rename Event
        /// </summary>
        private readonly List<PathPair> _renameEventPathPair;
        /// <summary>
        /// For Delete Event
        /// </summary>
        private readonly List<PathPair> _deleteEventPathPair;

        public PathTable()
        {
            _createEventPathPair = new List<PathPair>();
            _updateEventPathPair = new List<PathPair>();
            _renameEventPathPair = new List<PathPair>();
            _deleteEventPathPair = new List<PathPair>();
        }
        /// <summary>
        /// Remove a particular pair of path with a type from the table.
        /// </summary>
        /// <param name="source">Source of propagation</param>
        /// <param name="dest">Destination of propagation</param>
        /// <param name="type">Type of table</param>
        /// <returns>The PathPair removed.</returns>
        public PathPair RemovePathPair(string source, string dest, TableType type)
        {
            PathPair returnPair = null;
            List<PathPair> usedTable = null;
            //Use different table based on type.
            switch (type)
            {
                case TableType.Create: usedTable = _createEventPathPair; break;
                case TableType.Update: usedTable = _updateEventPathPair; break;
                case TableType.Rename: usedTable = _renameEventPathPair; break;
                case TableType.Delete: usedTable = _deleteEventPathPair; break;

            }
            if (usedTable == null)
            {
                return null;
            }
            lock (this)
            {
                //Create a Copy of the Pathpair, 
                //Find a same PathPair and return it.
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
        /// <summary>
        /// Remove a particular pair of path with a type from the table.
        /// </summary>
        /// <param name="source">Source of propagation</param>
        /// <param name="dest">Destination of propagation</param>
        /// <param name="type">Type of table</param>
        /// <returns>true if something is removed. false if the pair does not exist.</returns>
        public bool JustPop(string source, string dest, TableType type)
        {
            PathPair returnPair = null;
            List<PathPair> usedTable = null;
            //Use different table based on table type
            switch (type)
            {
                case TableType.Create: usedTable = _createEventPathPair; break;
                case TableType.Update: usedTable = _updateEventPathPair; break;
                case TableType.Rename: usedTable = _renameEventPathPair; break;
                case TableType.Delete: usedTable = _deleteEventPathPair; break;
            }
            if (usedTable == null)
            {
                return false;
            }
            lock (this)
            {
                //Create a copy of the path pair.
                //Find a same PathPair and return it.
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
        /// <summary>
        /// Add a new PathPair
        /// </summary>
        /// <param name="source">Source of propagation</param>
        /// <param name="dest">Destination of propagation</param>
        /// <param name="type">Type of table</param>
        /// <returns>true if the pair is added, false if the pair isn't</returns>
        public bool AddPathPair(string source, string dest, TableType type)
        {
            List<PathPair> usedTable = null;
            //Use the table based on the table type.
            switch (type)
            {
                case TableType.Create: usedTable = _createEventPathPair; break;
                case TableType.Update: usedTable = _updateEventPathPair; break;
                case TableType.Rename: usedTable = _renameEventPathPair; break;
                case TableType.Delete: usedTable = _deleteEventPathPair; break;

            }
            if (usedTable == null)
            {
                return false;
            }
            lock (this)
            {
                //Check if the table already contain such a pair.
                PathPair pathPair = new PathPair(source, dest);
                if (!Contains(pathPair, type))
                {
                    usedTable.Add(pathPair);
                    return true;
                }

            }
            return false;
        }
        /// <summary>
        /// Check if the PathTable contain a particular pair.
        /// </summary>
        /// <param name="pair">The pair to check</param>
        /// <param name="type">The type of table to check</param>
        /// <returns>true if the PathTable contains the pair, false if the PathTable does not contain the pair</returns>
        public bool Contains(PathPair pair, TableType type)
        {
            List<PathPair> usedTable = null;
            switch (type)
            {
                case TableType.Create: usedTable = _createEventPathPair; break;
                case TableType.Update: usedTable = _updateEventPathPair; break;
                case TableType.Rename: usedTable = _renameEventPathPair; break;
                case TableType.Delete: usedTable = _deleteEventPathPair; break;
            }
            if (usedTable == null)
            {
                return false;
            }
            return usedTable.Contains(pair);
        }
        /// <summary>
        /// Return the number of path pair in the Path Table.
        /// </summary>
        public int Count
        {
            get { return _createEventPathPair.Count + _updateEventPathPair.Count + _renameEventPathPair.Count; }

        }
        public void ClearEntry()
        {
            lock (this)
            {
                _createEventPathPair.Clear();
                _updateEventPathPair.Clear();
                _renameEventPathPair.Clear();
                _deleteEventPathPair.Clear();
            }
        }

        public void PrintAll()
        {
            lock (this)
            {
                ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("Create Table : " + _createEventPathPair.Count);
                foreach (PathPair p in _createEventPathPair)
                {
                    ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("Source:"+ p.Source + "Destination:" + p.Dest);
                }
                ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("Update Table : " + _createEventPathPair.Count);
                foreach (PathPair p in _updateEventPathPair)
                {
                    ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("Source:" + p.Source + "Destination:" + p.Dest);
                }
                ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("Rename Table : " + _createEventPathPair.Count);
                foreach (PathPair p in _renameEventPathPair)
                {
                    ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("Source:" + p.Source + "Destination:" + p.Dest);
                }
                ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("Delete Table : " + _createEventPathPair.Count);
                foreach (PathPair p in _deleteEventPathPair)
                {
                    ServiceLocator.GetLogger(ServiceLocator.DEVELOPER_LOG).Write("Source:" + p.Source + "Destination:" + p.Dest);
                }

            }
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

}
