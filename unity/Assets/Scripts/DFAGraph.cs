using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DFAGraph {
    public class Node
    {
        public bool visited { get; set; }
        public Node shortestPrevious = null;
        public List<Edge> edges { get; set; }

        public Node()
        {
            visited = false;
        }

        //When you cut a wire, call currentPosition.Traverse(wire)
        public Node NextNode(Wire wire)
        {
            visited = true;
            foreach (Edge edge in edges)
            {
                if (edge.color == wire.color)
                    return edge.child;
            }
            //Don't know if we want an error state node or just return null
            return null;
        }

        public bool HasEdge(Color color)
        {
            foreach (Edge edge in edges)
            {
                if (edge.color == color)
                    return true;
            }
            return false;
        }

        public IEnumerable<Node> GetChildNodes()
        {
            return edges.Select(x => x.child);
        }

        public void UnvisitChildren()
        {
            visited = false;
            shortestPrevious = null;
            foreach (Node n in this.GetChildNodes())
            {
                if(n.visited)
                {
                    n.UnvisitChildren();
                }
            }
        }
    }

    public class Edge
    {
        public Color color { get; set; }
        public Node child { get; set; }
        public Node parent { get; set; }

        public Edge(Node parent, Color color, Node child)
        {
            this.parent = parent;
            this.child = child;
            this.color = color;
        }

    }

    public class Wire
    {
        public Color color { get; set; }
        public bool cut = false;

        public Wire(Color col)
        {
            color = col;
        }

    }
}