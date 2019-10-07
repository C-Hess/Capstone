using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DFAGraph
{
    public class DFANode : MonoBehaviour
    {

        public bool visited = false;
        public DFANode shortestPrevious = null;
        public List<DFAEdge> edges = new List<DFAEdge>();

        //When you cut a wire, call currentPosition.Traverse(wire)
        public DFANode NextNode(Wire wire)
        {
            visited = true;
            foreach (DFAEdge edge in edges)
            {
                if (edge.Color == wire.color)
                    return edge.child;
            }
            //Don't know if we want an error state node or just return null
            return null;
        }

        public bool HasEdge(Color color)
        {
            foreach (DFAEdge edge in edges)
            {
                if (edge.Color == color)
                    return true;
            }
            return false;
        }

        public IEnumerable<DFANode> GetChildNodes()
        {
            return edges.Select(x => x.child);
        }

        public void UnvisitChildren()
        {
            visited = false;
            shortestPrevious = null;
            foreach (DFANode n in this.GetChildNodes())
            {
                if (n.visited)
                {
                    n.UnvisitChildren();
                }
            }
        }
    }
}

