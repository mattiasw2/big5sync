using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging.RecursionGraph
{
    class GraphNode
    {
        private List<string> _tagNames;

        public List<string> TagNames
        {
            get { return _tagNames; }
            set { _tagNames = value; }
        }
        private List<string> _paths;

        public List<string> Paths
        {
            get { return _paths; }
            set { _paths = value; }
        }
        private List<GraphNode> _connectedNodes;

        public List<GraphNode> ConnectedNodes
        {
            get { return _connectedNodes; }
            set { _connectedNodes = value; }
        }
        private List<GraphNode> _testingNodes;

        public List<GraphNode> TestingNodes
        {
            get { return _testingNodes; }
            set { _testingNodes = value; }
        }

        private bool _visited;

        public bool Visited
        {
            get { return _visited; }
            set { _visited = value; }
        }

        public GraphNode()
        {
            this._tagNames = new List<string>();
            this._connectedNodes = new List<GraphNode>();
            this._paths = new List<string>();
            this._testingNodes = new List<GraphNode>();
            _visited = false;
        }

        public void RemoveAllTempNode()
        {
            _testingNodes.Clear();
        }
        public void ConfirmAllTempNode()
        {
            _connectedNodes.AddRange(_testingNodes);
            RemoveAllTempNode();
        }

        
    }
}
