using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    List<Edge> edges;
    bool visited;
    public bool Visited { get => visited; set => visited = value; }
    public List<Edge> Edges { get => edges; set => edges = value; }


    public Node()
    {
        visited = false;        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //When you cut a wire, call currentPosition.Traverse(wire)
    public Node NextNode(Wire wire)
    {
        visited = true;
        foreach (Edge edge in Edges)
        {
            if (edge.Color == wire.Color)
                return edge.Child;
        }
        //Don't know if we want an error state node or just return null
        return null;
    }

    public bool HasEdge(string color)
    {
        foreach (Edge edge in Edges)
        {
            if (edge.Color == color)
                return true;
        }
        return false;
    }

}
public class Edge: MonoBehaviour
{
    string color;
    Node parent;
    Node child;
    public Edge(Node parent,string col,Node child)
    {
        this.parent = parent;
        this.child = child;
        color = col;
    }
    public string Color { get => color; set => color = value; }
    public Node Child { get => child; set => child = value; }
}

public class Wire : MonoBehaviour
{
    string color;
    bool cut;
    public Wire(string col)
    {
        color = col;
    }
    public string Color { get => color; set => color = value; }
    public bool Cut { get => cut; set => cut = value; }
}

public class GameState : MonoBehaviour
{
    int levelNumber;
    int score;
    int time;
    List<Wire> wires;
    Node currentPosition;
    Node endNode;

    public GameState(int level)
    {
        System.Random rnd = new System.Random();
        levelNumber = level;
        wires = new List<Wire>();
        string[] colors = { "Red", "Green", "Blue" };
        for(int i=0;i<15;i++)
        {
            wires[i] = new Wire(colors[rnd.Next(3)]);
        }

        FirstLevel FL = new FirstLevel();
        currentPosition = FL.Dfa[0];
        endNode = FL.Dfa[6];
    }
    
    public void Traverse(Wire wire)
    {
        currentPosition = currentPosition.NextNode(wire);
        if (currentPosition == null)
        {
            //Bomb explodes, game over
            //TODO: implement this
        }
        if (currentPosition == endNode)
        {
            //Win
            //TODO: implement winning
        }
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


}

public class FirstLevel: MonoBehaviour
{
    List<Node> dfa;
    List<Edge> edgeList;
    public FirstLevel()
    {
        Dfa = new List<Node>();
        for (int i = 0; i < 7; i++)
        {
            Dfa[i] = new Node();
        }
        edgeList = new List<Edge>();
        edgeList[0] = new Edge(Dfa[0], "Red", Dfa[1]);
        edgeList[1] = new Edge(Dfa[0], "Blue", Dfa[2]);

        edgeList[2] = new Edge(Dfa[1], "Blue", Dfa[3]);

        edgeList[3] = new Edge(Dfa[2], "Red", Dfa[4]);

        edgeList[4] = new Edge(Dfa[3], "Red", Dfa[5]);
        edgeList[5] = new Edge(Dfa[3], "Green", Dfa[2]);

        edgeList[6] = new Edge(Dfa[4], "Blue", Dfa[5]);
        edgeList[7] = new Edge(Dfa[4], "Green", Dfa[1]);

        edgeList[8] = new Edge(Dfa[5], "Red", Dfa[5]);
        edgeList[9] = new Edge(Dfa[5], "Blue", Dfa[2]);
        edgeList[10] = new Edge(Dfa[5], "Green", Dfa[6]);

        edgeList[11] = new Edge(Dfa[6], "Red", Dfa[6]);
        edgeList[12] = new Edge(Dfa[6], "Blue", Dfa[6]);
        edgeList[13] = new Edge(Dfa[6], "Green", Dfa[6]);

        Dfa[0].Edges = edgeList.GetRange(0, 2);
        Dfa[1].Edges = edgeList.GetRange(2, 1);
        Dfa[2].Edges = edgeList.GetRange(3, 1);
        Dfa[3].Edges = edgeList.GetRange(4, 2);
        Dfa[4].Edges = edgeList.GetRange(6, 2);
        Dfa[5].Edges = edgeList.GetRange(8, 3);
        Dfa[6].Edges = edgeList.GetRange(11, 3);
    }

    public List<Node> Dfa { get => dfa; set => dfa = value; }
}