using DFAGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    public int levelNumber;
    public int time;
    public List<Wire> wires;
    public List<GameObject> wire;
    public Node currentPosition;
    public Node endNode;

    public float restart = 10f;
    private float startTime;
    public int scoreTime = 0;

    //running = 1, transition = 2, gameover = 3
    public int gameState = 1;


    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;

        System.Random rnd = new System.Random();
        levelNumber = 1;
        wires = new List<Wire>();
        Color[] colors = { new Color(1, 0, 0), new Color(0, 1, 0), new Color(0, 0, 1) };
        for (int i = 0; i < 15; i++)
        {
            wires.Add(new Wire(colors[rnd.Next(2)]));
        }

        FirstLevel FL = new FirstLevel();
        currentPosition = FL.dfa[0];
        endNode = FL.dfa[6];

        var edges = FindMinPath();
        foreach(var e in edges)
        {
            Debug.Log(e.color);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(gameState == 1)
        {
            scoreTime = (int) Time.time - (int) startTime;

            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    MeshCollider mc = hit.collider as MeshCollider;
                    if (mc != null)
                    {
                        Destroy(mc.gameObject);
                        Debug.Log("I hit a wire.");
                        Debug.Log(hit.collider.gameObject);
                    }
                }
            }
        }
        if(gameState == 2)
        {
            
        }
        if (gameState == 3)
        {

        }
    }

    public void LevelWon()
    {
        Debug.Log("Level Complete!");
        levelNumber++;
        gameState = 2;
        Invoke("Restart", restart);
    }

    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        Invoke("Restart", restart);
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void Traverse(Wire wire)
    {
        currentPosition = currentPosition.NextNode(wire);
        if (currentPosition == null)
        {
            //Bomb explodes, game over
            //TODO: implement this
            gameState = 3;
        }
        if (currentPosition == endNode)
        {
            //Win
            //TODO: implement winning
        }
    }

   
    public List<Edge> FindMinPath()
    {
        currentPosition.UnvisitChildren();

        var nodes = new Stack<Tuple<Node, Node>>();
        nodes.Push(Tuple.Create<Node, Node>(currentPosition, null));

        while(nodes.Count > 0)
        {
            var current = nodes.Pop();

            current.Item1.visited = true;
            current.Item1.shortestPrevious = current.Item2;

            if(current.Item1 == endNode)
            {
                break;
            }
            else
            {
                foreach (Node n in current.Item1.GetChildNodes())
                {
                    if (!n.visited)
                    {
                        nodes.Push(Tuple.Create(n, current.Item1));
                    }
                }
            }
        }

        var shortestPath = new List<Node>();
        var backtrackCurrent = endNode;
        while(backtrackCurrent != null)
        {
            shortestPath.Add(backtrackCurrent);
            backtrackCurrent = backtrackCurrent.shortestPrevious;
        }
        shortestPath.Reverse();

        var edgesForPath = new List<Edge>();
        for(int i = 0; i < shortestPath.Count - 1; i ++)
        {
            Node currNode = shortestPath[i];
            Node nextNode = shortestPath[i + 1];
            edgesForPath.Add(currNode.edges.Find(x => x.child == nextNode));
        }

        return edgesForPath;
    }


}

class FirstLevel
{
    public List<Node> dfa;
    List<Edge> edgeList;

    public FirstLevel()
    {

        Color red = new Color(1, 0, 0);
        Color green = new Color(0, 1, 0);
        Color blue = new Color(0, 0, 1);

        dfa = new List<Node>();
        for (int i = 0; i < 7; i++)
        {
            dfa.Add(new Node());
        }
        edgeList = new List<Edge>();
        edgeList.Add(new Edge(dfa[0], red, dfa[1]));
        edgeList.Add(new Edge(dfa[0], blue, dfa[2]));

        edgeList.Add(new Edge(dfa[1], blue, dfa[3]));

        edgeList.Add(new Edge(dfa[2], red, dfa[4]));

        edgeList.Add(new Edge(dfa[3], red, dfa[5]));
        edgeList.Add(new Edge(dfa[3], green, dfa[2]));

        edgeList.Add(new Edge(dfa[4], blue, dfa[5]));
        edgeList.Add(new Edge(dfa[4], green, dfa[1]));

        edgeList.Add(new Edge(dfa[5], red, dfa[5]));
        edgeList.Add(new Edge(dfa[5], blue, dfa[2]));
        edgeList.Add(new Edge(dfa[5], green, dfa[6]));

        edgeList.Add(new Edge(dfa[6], red, dfa[6]));
        edgeList.Add(new Edge(dfa[6], blue, dfa[6]));
        edgeList.Add(new Edge(dfa[6], green, dfa[6]));

        dfa[0].edges = edgeList.GetRange(0, 2);
        dfa[1].edges = edgeList.GetRange(2, 1);
        dfa[2].edges = edgeList.GetRange(3, 1);
        dfa[3].edges = edgeList.GetRange(4, 2);
        dfa[4].edges = edgeList.GetRange(6, 2);
        dfa[5].edges = edgeList.GetRange(8, 3);
        dfa[6].edges = edgeList.GetRange(11, 3);
    }

}