using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    List<Edge> edges;
    bool visited;
    public bool Visited { get => visited; set => visited = value; }
    public List<Edge> Edges { get => edges; set => edges = value; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //When you cut a wire, call currentPosition.Traverse(wire)
    public Node Traverse(Wire wire)
    {
        foreach (Edge edge in Edges)
        {
            if (edge.Color == wire.Color)
                return edge.Child;
        }
        //Don't know if we want an error state node or just return null
        return null;
    }

}
public class Edge: MonoBehaviour
{
    string color;
    Node parent;
    Node child;

    public string Color { get => color; set => color = value; }
    public Node Child { get => child; set => child = value; }
}

public class Wire : MonoBehaviour
{
    string color;
    bool cut;
    public string Color { get => color; set => color = value; }
    public bool Cut { get => cut; set => cut = value; }
}

public class GameState : MonoBehaviour
{
    int levelNumber;
    int score;
    int time;
    Node currentPosition;
    Node endNode;

    public bool didWin()
    {
        return (currentPosition == endNode);
    }

    /*
     * //This is going to be a mess and i'm not doing it until we get everything else settled
    public List<string> findMinPath()
    {
        HashSet<testc> closed = new HashSet<testc>();
        Stack fringe = new Stack();
        Tuple<Node,List<Edge>>path = new Tuple<Node,List<Edge>>();
        Node childnode;
        closed.Add(currentPosition);
        foreach(Edge edge in currentPosition.Edges)
        {
        
            childnode = edge.Child;
            
        }
    }*/

    public void Traverse(Wire wire)
    {
        currentPosition = currentPosition.Traverse(wire);
        if (currentPosition == null)
        {
            //Bomb explodes, game over
            //TODO: implement this
        }
    }
}