using DFAGraph;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public int levelNumber;
    public int score;
    public int time;
    public List<Wire> wires;
    public DFANode startNode;
    public DFANode currentPosition;
    public DFANode endNode;
    public Canvas canvas;
    public GameObject dfaNodePrefab;
    public GameObject dfaEdgePrefab;

    // Start is called before the first frame update
    void Start()
    {
        System.Random rnd = new System.Random();
        levelNumber = 1;
        wires = new List<Wire>();
        Color[] colors = { new Color(1, 0, 0), new Color(0, 1, 0), new Color(0, 0, 1) };
        for (int i = 0; i < 15; i++)
        {
            wires.Add(new Wire(colors[rnd.Next(2)]));
        }

        var dfa = GenerateLevel();
        startNode = dfa[0];
        currentPosition = startNode;
        endNode = dfa[6];

        var edges = FindMinPath();
        foreach (var e in edges)
        {
            Debug.Log(e.Color);
        }

    }

    // Update is called once per frame
    void Update()
    {

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


    public List<DFAEdge> FindMinPath()
    {
        startNode.UnvisitChildren();

        var nodes = new Stack<Tuple<DFANode, DFANode>>();
        nodes.Push(Tuple.Create<DFANode, DFANode>(startNode, null));

        while (nodes.Count > 0)
        {
            var current = nodes.Pop();

            current.Item1.visited = true;
            current.Item1.shortestPrevious = current.Item2;

            if (current.Item1 == endNode)
            {
                break;
            }
            else
            {
                foreach (DFANode n in current.Item1.GetChildNodes())
                {
                    if (!n.visited)
                    {
                        nodes.Push(Tuple.Create(n, current.Item1));
                    }
                }
            }
        }

        var shortestPath = new List<DFANode>();
        var backtrackCurrent = endNode;
        while (backtrackCurrent != null)
        {
            shortestPath.Add(backtrackCurrent);
            backtrackCurrent = backtrackCurrent.shortestPrevious;
        }
        shortestPath.Reverse();

        var edgesForPath = new List<DFAEdge>();
        for (int i = 0; i < shortestPath.Count - 1; i++)
        {
            DFANode currNode = shortestPath[i];
            DFANode nextNode = shortestPath[i + 1];
            edgesForPath.Add(currNode.edges.Find(x => x.child == nextNode));
        }

        return edgesForPath;
    }

    public DFANode SpawnNode(Vector3 position)
    {
        var newObj = Instantiate(this.dfaNodePrefab, canvas.transform);
        newObj.transform.position = position;
        return newObj.GetComponent<DFANode>();
    }

    public DFAEdge SpawnEdge(DFANode parent, DFANode child, Color color)
    {
        var newObj = Instantiate(this.dfaEdgePrefab, canvas.transform);
        var edge = newObj.GetComponent<DFAEdge>();
        edge.parent = parent;
        edge.child = child;
        edge.Color = color;

        parent.edges.Add(edge);
        child.edges.Add(edge);

        var parentPos = parent.gameObject.transform.position;
        var childPos = child.gameObject.transform.position;

        var edgeRectTrans = newObj.GetComponent<RectTransform>();
        edgeRectTrans.sizeDelta = new Vector2(Vector3.Distance(parentPos, childPos), 5);
        Debug.Log("New Edge: "); 
        Debug.Log(parentPos);
        Debug.Log(childPos);
        Debug.Log(Vector2.Angle(childPos - parentPos, childPos));

        newObj.transform.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(parentPos - childPos, parent.gameObject.transform.right, parent.gameObject.transform.up));

        edge.transform.position = (parentPos + childPos) / 2;
        return edge;
    }

    public List<DFANode> GenerateLevel()
    {

        Color red = new Color(1, 0, 0);
        Color green = new Color(0, 1, 0);
        Color blue = new Color(0, 0, 1);

        var dfa = new List<DFANode>();

        float minX = canvas.GetComponent<RectTransform>().position.x + canvas.GetComponent<RectTransform>().rect.xMin;
        float maxY = canvas.GetComponent<RectTransform>().position.y + canvas.GetComponent<RectTransform>().rect.yMax;
        float z = canvas.GetComponent<RectTransform>().position.z;

        int nodeDiam = 100;
        int padding = 40;
        Vector3 origin = new Vector3(minX + nodeDiam/2 + padding/2, maxY - nodeDiam / 2 - padding / 2, z);
        Vector3 xOffset = new Vector3(nodeDiam + padding, 0, 0);
        Vector3 yOffset = new Vector3(0, -nodeDiam - padding, 0);

        int x = 0;
        int y = 0;

        for (int i = 0; i < 7; i++)
        {
            dfa.Add(SpawnNode(origin + xOffset * x + yOffset * y));
            x++;
            if(x > 2)
            {
                x = 0;
                y++;
            }
        }

        SpawnEdge(dfa[0], dfa[1], red);
        SpawnEdge(dfa[0], dfa[2], blue);

        SpawnEdge(dfa[1], dfa[3], blue);

        SpawnEdge(dfa[2], dfa[4], red);

        SpawnEdge(dfa[3], dfa[5], red);
        SpawnEdge(dfa[3], dfa[2], green);

        SpawnEdge(dfa[4], dfa[5], blue);
        SpawnEdge(dfa[4], dfa[1], green);

        SpawnEdge(dfa[5], dfa[5], red);
        SpawnEdge(dfa[5], dfa[2], blue);
        SpawnEdge(dfa[5], dfa[6], green);

        SpawnEdge(dfa[6], dfa[6], red);
        SpawnEdge(dfa[6], dfa[6], blue);
        SpawnEdge(dfa[6], dfa[6], green);

        return dfa;
    }

}