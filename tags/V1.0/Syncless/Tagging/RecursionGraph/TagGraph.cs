using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging.RecursionGraph
{
    class TagGraph
    {
        private List<GraphNode> _nodes;

        public TagGraph()
        {
            _nodes = new List<GraphNode>();
        }

        public bool Add(string tagName, string path)
        {
            GraphNode node = FindNode(tagName);
            if (node != null)
            {
                node.Paths.Add(path);
            }
            else
            {
                node = new GraphNode();
                node.TagNames.Add(tagName);
                node.Paths.Add(path);
            }
            CreateLink(node);
            CheckRecursion(node);

            return true;
        }

        private GraphNode FindNode(string tagName)
        {
            foreach (GraphNode node in _nodes)
            {
                if (node.TagNames.Contains(tagName))
                {
                    return node;
                }
            }
            return null;
        }
        private void CreateLink(GraphNode n)
        {

        }
        private bool CheckRecursion(GraphNode n)
        {
            return TryReach(n, n, n);
        }
        private bool TryReach(GraphNode startPoint, GraphNode endPoint, GraphNode currentNode)
        {
            if (currentNode == endPoint && currentNode.Visited)
            {
                return true;
            }
            currentNode.Visited = true;
            foreach (GraphNode node in currentNode.ConnectedNodes)
            {
                if (!node.Visited)
                {
                    bool reached = TryReach(startPoint, endPoint, node); 
                    if(reached){
                        return true;
                    }
                }
            }
            foreach (GraphNode node in currentNode.TestingNodes)
            {
                if(!node.Visited){
                    bool reached = TryReach(startPoint,endPoint,node);
                    if(reached){
                        return true;
                    }
                }

            }
            return false;
        }
    }
}
