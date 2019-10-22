using DFAGraph;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class GameState : MonoBehaviour
{
    public int levelNumber;
    private int score;
    public Text scoreText;
    public int time;
    private DFANode startNode;
    private DFANode currentPosition;
    private DFANode endNode;
    private List<DFANode> allNodes = new List<DFANode>();
    public Canvas canvas;
    public GameObject dfaNodePrefab;
    public GameObject dfaEdgePrefab;
    public GameObject bomb;
    public List<GameObject> WirePrefab;
    public UIStateManager uiManager;
    public LCDController lcdController;
    
    public float restart = 1f;

    // Start is called before the first frame update
    void Start()
    {
        levelNumber = 1;

        var dfa = GenerateLevel();
        startNode = dfa[0];
        startNode.IsCurrent = true;
        currentPosition = startNode;
        endNode = dfa[6];
        endNode.gameObject.GetComponent<Image>().color = Color.red;

        var edges = FindMinPath();
        edges = edges.OrderBy(x => UnityEngine.Random.Range(0, edges.Count - 1)).ToList();
        for(int i = 0; i < edges.Count; i ++)
        {
            var newWire = Instantiate(WirePrefab[i], bomb.transform);
            newWire.GetComponent<Renderer>().material.color = edges[i].GetColor();
            newWire.GetComponent<DFAWire>().color = edges[i].GetColorStr();
        }

    }

    public void LevelWon()
    {
        Debug.Log("Level Complete!");
        lcdController.StopTimer();
        uiManager.SwitchWin();
    }

    public void GameOver()
    {
        Debug.Log("Failure!");
        lcdController.StopTimer();
        uiManager.SwitchLose();    }

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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                MeshCollider mc = hit.collider as MeshCollider;
                if (mc != null && hit.collider.gameObject.CompareTag("Selectable"))
                {
                    Traverse(mc.gameObject);
                    mc.gameObject.SetActive(false); //instead of destroying the object, im doing this so it just deactivates it.
                }
            }
        }
    }

    public void Traverse(GameObject wire)
    {
        currentPosition.IsCurrent = false;
        currentPosition = currentPosition.NextNode(wire.GetComponent<DFAWire>().color);
        if (currentPosition == null)
        {
            GameOver();
        }
        else
        {
            currentPosition.IsCurrent = true;
            if (currentPosition == endNode)
            {
                LevelWon();
            }
        }

    }


    public List<DFAEdge> FindMinPath()
    {
        ResetVisited();

        var nodes = new Queue<Tuple<DFANode, DFANode>>();
        nodes.Enqueue(Tuple.Create<DFANode, DFANode>(startNode, null));

        while (nodes.Count > 0)
        {
            var current = nodes.Dequeue();

            current.Item1.Visited = true;
            current.Item1.shortestPrevious = current.Item2;

            if (current.Item1 == endNode)
            {
                break;
            }
            else
            {
                foreach (DFANode n in current.Item1.GetChildNodes())
                {
                    if (!n.Visited)
                    {
                        nodes.Enqueue(Tuple.Create(n, current.Item1));
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
        ResetVisited();

        return edgesForPath;
    }

    public DFANode SpawnNode(Vector3 position)
    {
        var newObj = Instantiate(this.dfaNodePrefab, canvas.transform);
        newObj.transform.position = position;
        allNodes.Add(newObj.GetComponent<DFANode>());
        return newObj.GetComponent<DFANode>();
    }

    public DFAEdge SpawnEdge(DFANode parent, DFANode child, Color color, string colorName)
    {
        var newObj = Instantiate(this.dfaEdgePrefab, canvas.transform);
        var edge = newObj.GetComponent<DFAEdge>();
        edge.parent = parent;
        edge.child = child;
        edge.SetColor(color, colorName);

        parent.edges.Add(edge);
        //child.edges.Add(edge);

        var parentPos = parent.gameObject.transform.position;
        var childPos = child.gameObject.transform.position;

        var edgeRectTrans = newObj.GetComponent<RectTransform>();
        edgeRectTrans.sizeDelta = new Vector2(Vector3.Distance(parentPos, childPos), 60);

        newObj.transform.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(parentPos - childPos, parent.gameObject.transform.right, -parent.gameObject.transform.forward));

        edge.transform.position = (parentPos + childPos) / 2;
        return edge;
    }

    private void ResetVisited()
    {
        foreach (var node in allNodes) {
            node.Visited = false;
        }
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

        SpawnEdge(dfa[0], dfa[1], red, "red");
        SpawnEdge(dfa[0], dfa[2], blue, "blue");

        SpawnEdge(dfa[1], dfa[3], blue, "blue");

        SpawnEdge(dfa[2], dfa[4], red, "red");

        SpawnEdge(dfa[3], dfa[5], red, "red");
        SpawnEdge(dfa[3], dfa[2], green, "green");

        SpawnEdge(dfa[4], dfa[5], blue, "blue");
        SpawnEdge(dfa[4], dfa[1], green, "green");

        SpawnEdge(dfa[5], dfa[2], blue, "blue");
        SpawnEdge(dfa[5], dfa[6], green, "green");

        return dfa;
    }

}
