﻿using DFAGraph;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class GameState : MonoBehaviour
{
    public int time;
    private DFANode startNode;
    private DFANode currentPosition;
    private DFANode endNode;
    private List<DFANode> allNodes = new List<DFANode>();
    private List<DFAEdge> allEdges = new List<DFAEdge>();

    public Canvas canvas;
    public GameObject dfaNodePrefab;
    public GameObject dfaEdgePrefab;
    public GameObject bomb;
    public List<GameObject> possibleWires;
    public UIStateManager uiManager;
    public LCDController lcdController;

    public int levelNumber = 0;
    public Text levelNumberText;
    public int score = 0;
    public Text scoreText;
    public Text endScoreText;
    public Text endLevelNumberText;


    public float repulseCoeff = 3.2e+07f;
    public float edgeRepulseCoeff = 3.2e+07f;
    public float attractionCoeff = 1000.0f;
    public float currMaxVelocity = 10.0f;

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
            var newWire = Instantiate(possibleWires[i], bomb.transform);
            newWire.GetComponent<Renderer>().material.color = edges[i].GetColor();
            newWire.GetComponent<DFAWire>().color = edges[i].GetColorStr();
        }

    }

    public void LevelWon()
    {
        Debug.Log("Level Complete!");
        lcdController.StopTimer();
        uiManager.SwitchWin();
        levelNumberText.text = "Level " + (levelNumber).ToString() + " Completed!";
        levelNumber += 1;
        score = (int) lcdController.InitialTime;
        scoreText.text = "Score" + score.ToString();
    }

    public void GameOver()
    {
        Debug.Log("Failure!");
        lcdController.StopTimer();
        uiManager.SwitchLose();
        endScoreText.text = "Total Score: " + score.ToString();
        endLevelNumberText.text = "Level " + (levelNumber).ToString() + " Failed.";
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

    void FixedUpdate()
    {
        float minX = canvas.GetComponent<RectTransform>().position.x + canvas.GetComponent<RectTransform>().rect.xMin;
        float maxX = canvas.GetComponent<RectTransform>().position.x + canvas.GetComponent<RectTransform>().rect.xMax;
        float minY = canvas.GetComponent<RectTransform>().position.y + canvas.GetComponent<RectTransform>().rect.yMin;
        float maxY = canvas.GetComponent<RectTransform>().position.y + canvas.GetComponent<RectTransform>().rect.yMax;

        Vector3[] canvasCorners = new Vector3[]
        {
            new Vector3(minX, minY),
            new Vector3(maxX, minY),
            new Vector3(maxX, maxY),
            new Vector3(minX, maxY),
        };

        for (int i = 0; i < canvasCorners.Length; i ++)
        {
            canvasCorners[i] = canvas.transform.TransformPoint(canvasCorners[i]);
        }

        var velocities = new List<Vector3>();
        for (int n1 = 0; n1 < allNodes.Count; n1++)
        {
            Vector3 velocity = new Vector3();

            for (int n2 = 0; n2 < allNodes.Count; n2++)
            {
                if (n1 == n2) continue;
                float distance = Vector3.Distance(allNodes[n1].gameObject.transform.localPosition, allNodes[n2].gameObject.transform.localPosition);
                Vector3 repulseDir = (allNodes[n1].gameObject.transform.localPosition - allNodes[n2].gameObject.transform.localPosition).normalized;
                Vector3 force = repulseDir * (repulseCoeff / (distance * distance));
                velocity += force * Time.fixedDeltaTime;
            }

            velocities.Add(velocity);
        }

        for (int n1 = 0; n1 < allNodes.Count; n1++)
        {
            Vector3 velocity = new Vector3();

            for (int e = 0; e < allNodes[n1].edges.Count; e++)
            {
                DFANode connectedNode = allNodes[n1].edges[e].child;
                if(connectedNode == allNodes[n1])
                {
                    connectedNode = allNodes[n1].edges[e].parent;
                }
                

                Vector3 attractionDir = (connectedNode.gameObject.transform.localPosition - allNodes[n1].gameObject.transform.localPosition).normalized;
                Vector3 force = attractionDir * attractionCoeff;
                velocity += force * Time.fixedDeltaTime;
            }

            for (int i = 0; i < 4; i++)
            {
                Vector3 corner1 = canvasCorners[i];
                int nextCorner = i + 1;
                if (nextCorner >= 4)
                {
                    nextCorner -= 4;
                }
                Vector3 corner2 = canvasCorners[nextCorner];

                Vector3 edge = corner2 - corner1;


                Vector3 cornerDir = allNodes[n1].transform.position - corner1;


                Vector3 edgePerp = edge.normalized * Vector3.Dot(edge, cornerDir) / edge.magnitude;


                Vector3 forceDir = (cornerDir - edgePerp).normalized;

                float distance = Vector3.Distance(cornerDir, edgePerp);


                Vector3 force = forceDir * (edgeRepulseCoeff / (distance * distance * distance));

                velocity += new Vector3(force.x, force.z, 0) * Time.fixedDeltaTime;
           
            }

            velocities[n1] += velocity;


        }

        for (int n1 = 0; n1 < allNodes.Count; n1++)
        {

            allNodes[n1].transform.localPosition += Mathf.Min(velocities[n1].magnitude, currMaxVelocity) * velocities[n1].normalized;
        }

        foreach(DFAEdge e in allEdges)
        {
            RerenderEdge(e);
        }

        //currMaxVelocity *= 0.99f;
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

    public DFANode SpawnNode(Vector3 localPosition)
    {
        var newObj = Instantiate(this.dfaNodePrefab, canvas.transform);
        newObj.transform.localPosition = localPosition;
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
        child.edges.Add(edge);

        var parentPos = parent.gameObject.transform.localPosition;
        var childPos = child.gameObject.transform.localPosition;

        var edgeRectTrans = newObj.GetComponent<RectTransform>();
        edgeRectTrans.sizeDelta = new Vector2(Vector3.Distance(parentPos, childPos), 60);

        edge.transform.rotation = Quaternion.Euler(90, 0, Vector3.SignedAngle(parentPos - childPos, parent.gameObject.transform.right, -parent.gameObject.transform.forward));
        edge.transform.localPosition = (parentPos + childPos) / 2;
        allEdges.Add(edge);
        return edge;
    }

    public void RerenderEdge(DFAEdge edge)
    {
        var parent = edge.parent;
        var child = edge.child;

        var parentPos = parent.gameObject.transform.localPosition;
        var childPos = child.gameObject.transform.localPosition;

        var edgeRectTrans = edge.gameObject.GetComponent<RectTransform>();
        edgeRectTrans.sizeDelta = new Vector2(Vector3.Distance(parentPos, childPos), 60);

        edge.gameObject.transform.rotation = Quaternion.Euler(-90, 0, Vector3.SignedAngle(parentPos - childPos, parent.gameObject.transform.right, -parent.gameObject.transform.forward));

        edge.transform.localPosition = (parentPos + childPos) / 2;
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
        float maxX = canvas.GetComponent<RectTransform>().position.x + canvas.GetComponent<RectTransform>().rect.xMax;
        float minY = canvas.GetComponent<RectTransform>().position.y + canvas.GetComponent<RectTransform>().rect.yMin;
        float maxY = canvas.GetComponent<RectTransform>().position.y + canvas.GetComponent<RectTransform>().rect.yMax;

        float z = 0;

        int nodeDiam = 1;
        int padding = 1;
        Vector3 origin = new Vector3((minX + maxX)/2, (minY + maxY) / 2, z);
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
