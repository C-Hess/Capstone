using DFAGraph;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class GameState : MonoBehaviour
{
    [System.Serializable]
    public class WireColor
    {
        public Color color;
        public String label;
    }


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
    public List<GameObject> jumpers;
    public WireColor[] allWireColors = new WireColor[6];
    public UIStateManager uiManager;
    public LCDController lcdController;

    public int mutationsPerLevel = 5;
    public int levelNumber = 1;
    public Text levelNumberText;
    public int score = 0;
    public Text scoreText;
    public Text endScoreText;
    public Text endLevelNumberText;

    public float repulseCoeff = 3.2e+07f;
    public float edgeRepulseCoeff = 6400f;
    public float attractionCoeff = 1000.0f;
    public float initMaxVelocity = 10.0f;
    private float currMaxVelocity;

    public float restart = 1f;
    private float timer = 0.0f;
    private float levelStartTime = 0.0f;
    private float newStartTime = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
        NextLevel();
    }

    public void NextLevel()
    {
        uiManager.SwitchGame();
        currMaxVelocity = initMaxVelocity;

        levelNumber++;
        levelStartTime = Mathf.Min(90, 30 + 1.5f * levelNumber-1);
        newStartTime = levelStartTime;
        timer = 0;

        foreach (var node in allNodes)
        {
            Destroy(node.gameObject);
        }
        allNodes.Clear();

        foreach (var edge in allEdges)
        {
            Destroy(edge.gameObject);
        }
        allEdges.Clear();

        GenerateLevel(levelNumber, mutationsPerLevel);

        startNode = allNodes[0];
        startNode.IsCurrent = true;
        currentPosition = startNode;
        endNode = allNodes[6];
        endNode.gameObject.GetComponent<Image>().color = Color.red;

        var edges = FindMinPath();
        edges = edges.OrderBy(x => UnityEngine.Random.Range(0, edges.Count - 1)).ToList();
        for (int i = 0; i < edges.Count; i++)
        {
            var newWire = Instantiate(possibleWires[i], bomb.transform);
            newWire.GetComponent<Renderer>().material.color = edges[i].GetColor();
            newWire.GetComponent<DFAWire>().color = edges[i].GetColorStr();
        }
        
        for (int i = 0; i < 6; i++)
        {
            var newJumper = Instantiate(jumpers[i], bomb.transform);
            newJumper.GetComponent<Renderer>().material.color = allWireColors[i].color;
            newJumper.GetComponent<JumperColor>().color = allWireColors[i].label;
        }

        lcdController.SetTimer(levelStartTime);
    }

    public void LevelWon()
    {
        Debug.Log("Level Complete!");
        lcdController.StopTimer();
        uiManager.SwitchWin();
        levelNumberText.text = "Level " + (levelNumber).ToString() + " Completed!";
        levelNumber += 1;
        Debug.Log("This is the lcdcontroller initial time: "+ levelStartTime);
        Debug.Log("This is the time.deltatime variable"+ Time.deltaTime);
        Debug.Log("this is the score variable before:" + score);
        
        
        score = (int)Mathf.Round((levelStartTime - timer)*100);
        Debug.Log("This is the score variable after:" + score);
        scoreText.text = "Score: " + score.ToString();
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
        Score.scoreValue = 0;
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
        timer += Time.deltaTime;
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                MeshCollider mc = hit.collider as MeshCollider;
                if (mc != null && hit.collider.gameObject.CompareTag("Jumper"))
                {
                    Jumper(mc.gameObject);
                } else if (mc != null && hit.collider.gameObject.CompareTag("Selectable"))
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
                float distance = Vector3.Distance(allNodes[n1].gameObject.transform.localPosition, allNodes[n2].gameObject.transform.localPosition) + 0.000001f;
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
                if (connectedNode == allNodes[n1])
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
                float distance = Vector3.Distance(cornerDir, edgePerp) + 0.00001f;

                Vector3 force = forceDir * (edgeRepulseCoeff / (distance * distance * distance));

                velocity += new Vector3(force.x, force.z, 0) * Time.fixedDeltaTime;
            }
            velocities[n1] += velocity;
        }

        for (int n1 = 0; n1 < allNodes.Count; n1++)
        {
            allNodes[n1].transform.localPosition += Mathf.Min(velocities[n1].magnitude, currMaxVelocity) * velocities[n1].normalized;
        }

        foreach (DFAEdge e in allEdges)
        {
            if (e != null)
            {
                RerenderEdge(e);
            }
        }

        currMaxVelocity *= 0.9925f;
    }

    /**
     * This method progresses through the dfa state given a wire that got cut.
     * 
     * @param wire - is the wire object that got cut
     */
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

    //I think this is working but idk how to correctly subtract time in the LCDController
    public void Jumper(GameObject jumper)
    {
        if(levelStartTime - timer > 10)
        {
            lcdController.subtractTime(newStartTime - 10, levelStartTime - timer - 10);
            newStartTime -= 10;
            timer += 10;
        } else
        {
            GameOver();
        }

        currentPosition.IsCurrent = false;
        currentPosition = currentPosition.NextNode(jumper.GetComponent<JumperColor>().color);
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
        foreach (var node in allNodes)
        {
            node.Visited = false;
        }
    }

    public void GenerateLevel(int level, int mutationsPerLevel)
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
        Vector3 origin = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, z);
        Vector3 xOffset = new Vector3(nodeDiam + padding, 0, 0);
        Vector3 yOffset = new Vector3(0, -nodeDiam - padding, 0);

        int x = 0;
        int y = 0;

        for (int i = 0; i < 7; i++)
        {
            SpawnNode(origin + xOffset * x + yOffset * y);
            x++;
            if (x > 2)
            {
                x = 0;
                y++;
            }
        }

        SpawnEdge(allNodes[0], allNodes[1], red, "red");
        SpawnEdge(allNodes[0], allNodes[2], blue, "blue");

        SpawnEdge(allNodes[1], allNodes[3], blue, "blue");

        SpawnEdge(allNodes[2], allNodes[4], red, "red");

        SpawnEdge(allNodes[3], allNodes[5], red, "red");
        SpawnEdge(allNodes[3], allNodes[2], green, "green");

        SpawnEdge(allNodes[4], allNodes[5], blue, "blue");
        SpawnEdge(allNodes[4], allNodes[1], green, "green");

        SpawnEdge(allNodes[5], allNodes[2], blue, "blue");
        SpawnEdge(allNodes[5], allNodes[6], green, "green");
        
        mutateLevel(level, mutationsPerLevel);
    }
    public void mutateLevel(int level, int mutationsPerLevel)
    {
        Color red = new Color(1, 0, 0);
        Color green = new Color(0, 1, 0);
        Color blue = new Color(0, 0, 1);
        Color color = red;

        float z = 0;
        float minX = canvas.GetComponent<RectTransform>().position.x + canvas.GetComponent<RectTransform>().rect.xMin;
        float maxX = canvas.GetComponent<RectTransform>().position.x + canvas.GetComponent<RectTransform>().rect.xMax;
        float minY = canvas.GetComponent<RectTransform>().position.y + canvas.GetComponent<RectTransform>().rect.yMin;
        float maxY = canvas.GetComponent<RectTransform>().position.y + canvas.GetComponent<RectTransform>().rect.yMax;

        int nodeDiam = 1;
        int padding = 1;
        Vector3 origin = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, z);
        Vector3 xOffset = new Vector3(nodeDiam + padding, 0, 0);
        Vector3 yOffset = new Vector3(0, -nodeDiam - padding, 0);

        int columns = 3;
        int rows = 2;
        int rowPos = 2;
        int colPos = 0;

        string[] colors = { "red", "green", "blue" };
        DFANode parentNode = null;
        DFANode childNode = null;

        string edgeColor = "";

        for (int i = 0; i < level * mutationsPerLevel; i++)
        {
            if (UnityEngine.Random.Range(0, 2) == 0)
            //Create new edge
            {
                bool nodeHasColor = true;
                while (nodeHasColor)
                {
                    parentNode = allNodes[UnityEngine.Random.Range(0, allNodes.Count - 1)];
                    edgeColor = colors[UnityEngine.Random.Range(0, 3)];
                    nodeHasColor = parentNode.HasOutgoingEdge(edgeColor);
                }
                childNode = allNodes[UnityEngine.Random.Range(0, allNodes.Count)];


                switch (edgeColor)
                {
                    case "red":
                        color = red;
                        break;
                    case "green":
                        color = green;
                        break;
                    case "blue":
                        color = blue;
                        break;
                }

                SpawnEdge(parentNode, childNode, color, edgeColor);
            }
            else //split a current edge
            {

                do
                {
                    parentNode = allNodes[UnityEngine.Random.Range(0, allNodes.Count - 1)];
                } while (parentNode.edges.Count == 0);

                var edgeToSplit = parentNode.edges[UnityEngine.Random.Range(0, parentNode.edges.Count)];
                childNode = edgeToSplit.child;
                color = edgeToSplit.GetColor();
                edgeColor = edgeToSplit.GetColorStr();



                //vector stuff

                int x = 0;
                int y = 0;
                if (rows < columns)
                {
                    rowPos++;
                    x = rowPos;
                    y = rows + 1;
                    if (rowPos >= columns)
                    {
                        rows++;
                        colPos = 0;
                        rowPos = 0;
                    }
                }
                else
                {
                    colPos++;
                    x = columns + 1;
                    y = colPos;
                    if (colPos >= rows)
                    {
                        columns++;
                        colPos = 0;
                        rowPos = 0;
                    }
                }

                SpawnNode(origin + xOffset * x + yOffset * y);
                var newNode = allNodes[allNodes.Count - 1];

                //spawn an edge from parent to new
                SpawnEdge(parentNode, newNode, color, edgeColor);
                //spawn edge from new node to child
                edgeColor = colors[UnityEngine.Random.Range(0, 3)];
                switch (edgeColor)
                {
                    case "red":
                        color = red;
                        break;
                    case "green":
                        color = green;
                        break;
                    case "blue":
                        color = blue;
                        break;
                }
                SpawnEdge(newNode, childNode, color, edgeColor);
                //remove the edge to split

                allEdges.Remove(edgeToSplit);
                parentNode.edges.Remove(edgeToSplit);
                childNode.edges.Remove(edgeToSplit);
                Destroy(edgeToSplit.gameObject);
            }
        }
    }
}

