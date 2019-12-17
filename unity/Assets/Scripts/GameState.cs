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
        public string label;
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

    private List<GameObject> wires = new List<GameObject>();


    // Start is called before the first frame update
     /**
     * This method calls the next level function
     * it has a for loop to instantiate the jumper wires so that there are 6 created, and gives them all separate colors
     * 
     * @param none
     */
    void Start()
    {
        NextLevel();

        for (int i = 0; i < 6; i++)
        {
            var newJumper = Instantiate(jumpers[i], bomb.transform);
            newJumper.GetComponent<Renderer>().material.color = allWireColors[i].color;
            newJumper.GetComponent<JumperColor>().color = allWireColors[i].label;
        }
    }
    /**
    * This method destroys all the prior objects from the previous level such as nodes and edges of the dfa
    * it increases the timer by 1.5 seconds each level from the base of 30 seconds
    * it generates by calling the generate level function, which is given the level number and the amount of mutations per level
    * it sets the start node to the 0th node
    * it sets the end node to the 6th node
    * it sets the end node to color red to signify it is the end node
    * it calculates the edges based on a minimal path to traverse the DFA
    * and then it instantiates new wires to go along the new level
    * then it gives the lcd controller the new start time
    * @param  none
    */
    public void NextLevel()
    {
        uiManager.SwitchGame();
        currMaxVelocity = initMaxVelocity;

        levelNumber++;
        levelStartTime = Mathf.Min(99, 30 + 1.5f * (levelNumber - 1));
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
            newWire.GetComponent<BombWire>().color = edges[i].GetColorStr();
            wires.Add(newWire);
        }

        lcdController.SetTimer(levelStartTime);
    }
    /**
    * This method is called if the level is won
    * it stops the lcd timer
    * it toggles the win screen to show the user their score and prompt them to move on to the next level
    * it states which level was completed, and it increments the score based off the time remaining from the level multiplied by 100 and added to the total score
    * it outputs the score on the ui screen
    * it destroys and clears the wires
    * 
    * @param 
    */
    public void LevelWon()
    {
        lcdController.StopTimer();
        uiManager.SwitchWin();
        levelNumberText.text = "Level " + (levelNumber).ToString() + " Completed!";
        score += (int)Mathf.Round((levelStartTime - timer)*100);
        scoreText.text = "Score: " + score.ToString();

        foreach (var wire in wires)
        {
            Destroy(wire);
        }

        wires.Clear();

    }
    /**
    * This method is called if the user loses on their current level
    * it toggles the ui screen to the lose screen
    * It states the total score that the user achieved on their run and outputs it
    * it states what level that they reached
    * 
    * @param 
    */
    public void GameOver()
    {
        lcdController.StopTimer();
        uiManager.SwitchLose();
        endScoreText.text = "Total Score: " + score.ToString();
        endLevelNumberText.text = "Level " + (levelNumber).ToString() + " Failed.";
    }
    /**
    * This method is called when the user wants to restart the game, it sets the score back to 0, and invokes restart
    * 
    * @param 
    */
    public void RestartGame()
    {
        Score.scoreValue = 0;
        Invoke("Restart", restart);
    }
    /**
    * This method is called when the user wants to restart the game, it has the scene manager load the level 1 scene
    * 
    * @param 
    */
    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    /**
    * This method is called when the user wants to access the main menu
    * 
    * @param 
    */
    public void MainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    // Update is called once per frame
    /**
    * This method sets the timer variable to be equal to the timer variable plus the delta time, which is calculated each frame, to account for users with slow computers
    * every frame, it will send a ray cast, from the users mouse to where it is located on in the scene, if the ray hits a selectable object or a jumper cable, it will light up yellow
    * if the jumper is clicked on, it will call the jumper function, if the wire is clicked on, the current node of the dfa will traverse to whatever state corresponds to the wire that was cut
    * the wire's "set active" will be set to false, hiding the wire from the scene.
    * 
    * @param 
    */
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


    /**
     * Called every physics update frame. Will update the physics based node topology and will update the edge transformations
     */
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


        HashSet<DFANode> nodesTraversed = new HashSet<DFANode>();

        foreach (DFANode n in allNodes)
        {

            float delta = 0.0f;

            var edgesSortedByParentNodeRefHash = n.edges.OrderBy(x => {
                DFANode otherNode = x.parent;
                if (otherNode == n)
                {
                    otherNode = x.child;
                }
                return otherNode.GetHashCode();
             }).ToList();

            DFANode currentNode = null;

            foreach(DFAEdge e in edgesSortedByParentNodeRefHash)
            {
                DFANode otherNode = e.parent;

                if(otherNode == n)
                {
                    otherNode = e.child;
                }

                if(currentNode != otherNode)
                {
                    delta = 0;
                    currentNode = otherNode;
                }

                if (!nodesTraversed.Contains(otherNode))
                {
                    UpdateEdge(e, delta);
                    delta = -delta + (Mathf.Abs(delta) + 20);
                }

            }

            nodesTraversed.Add(n);
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
        if (uiManager.GetState() == UIStateManager.UIStates.GAME)
        {
            currentPosition.IsCurrent = false;
            currentPosition = currentPosition.NextNode(wire.GetComponent<BombWire>().color);
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
    }

    //I think this is working but idk how to correctly subtract time in the LCDController
    /**
    * This method takes the input on if the jumper wire was clicked on, if it was, then it subtracts 10 seconds from the timer if there is more than 10 seconds left on the timer
    * if there is less than 10 seconds left on the timer, it triggers the gameover function, due to there not being enough time left
    * 
    * @param jumper - the jumper wire that is clicked on
    */
    public void Jumper(GameObject jumper)
    {

        if (uiManager.GetState() == UIStateManager.UIStates.GAME)
        {
            if (levelStartTime - timer > 10)
            {
                timer += 10;
                lcdController.SetTimer(levelStartTime - timer);
            }
            else
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
    }

    /**
    * This method finds a minimal path from the start state to the ends state of the DFA
    * 
    * @param 
    */
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
    /**
    * This method spawns the nodes for the DFA
    * 
    * @param localPosition
    */
    public DFANode SpawnNode(Vector3 localPosition)
    {
        var newObj = Instantiate(this.dfaNodePrefab, canvas.transform);
        newObj.transform.localPosition = localPosition;
        allNodes.Add(newObj.GetComponent<DFANode>());
        return newObj.GetComponent<DFANode>();
    }
    /**
    * This method spawns the edges corresponding to the DFA that was generated
    * 
    * @param parent
    * 
    * @param child
    * 
    * @param color
    * 
    * @param colorName
    */
    public DFAEdge SpawnEdge(DFANode parent, DFANode child, WireColor wireColor)
    {
        var newObj = Instantiate(this.dfaEdgePrefab, canvas.transform);
        var edge = newObj.GetComponent<DFAEdge>();
        edge.parent = parent;
        edge.child = child;
        edge.SetColor(wireColor);

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
    /**
    * This method updates the position and rotation of the edges in the world space.
    * Should be called every physics update (fixedUpdate)
    * 
    * @param edge - The edge to update
    * @param delta - Offset to be applied so that multiple edges between two nodes do not
    * overlap
    */
    public void UpdateEdge(DFAEdge edge, float delta)
    {
        var parent = edge.parent;
        var child = edge.child;

        var parentPos = parent.gameObject.transform.localPosition;
        var childPos = child.gameObject.transform.localPosition;

        var normVect = Vector2.Perpendicular(childPos-parentPos).normalized;

        var offsetVect = normVect * delta;

        var edgeRectTrans = edge.gameObject.GetComponent<RectTransform>();
        edgeRectTrans.sizeDelta = new Vector2(Vector3.Distance(parentPos, childPos), 60);

        edge.gameObject.transform.rotation = Quaternion.Euler(-90, 0, Vector3.SignedAngle(parentPos - childPos, parent.gameObject.transform.right, -parent.gameObject.transform.forward));
        edge.transform.localPosition = ((parentPos + childPos) / 2) + (Vector3)offsetVect;
    }
    /**
    * This method sets all the visited nodes to false
    * 
    * @param localPosition
    */
    private void ResetVisited()
    {
        foreach (var node in allNodes)
        {
            node.Visited = false;
        }
    }
    /**
    * This method generates the level, given the input of the level, and the amount of mutations there should be per level
    * 
    * @param level - the level that the user is on
    * 
    * @param mutationsPerLevel - the amount of mutations alloted for the specific level
    */
    public void GenerateLevel(int level, int mutationsPerLevel)
    {

        WireColor blue = allWireColors[0];
        WireColor red = allWireColors[1];
        WireColor green = allWireColors[2];

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

        SpawnEdge(allNodes[0], allNodes[1], red);
        SpawnEdge(allNodes[0], allNodes[2], blue);

        SpawnEdge(allNodes[1], allNodes[3], blue);

        SpawnEdge(allNodes[2], allNodes[4], red);

        SpawnEdge(allNodes[3], allNodes[5], red);
        SpawnEdge(allNodes[3], allNodes[2], green);

        SpawnEdge(allNodes[4], allNodes[5], blue);
        SpawnEdge(allNodes[4], allNodes[1], green);

        SpawnEdge(allNodes[5], allNodes[2], blue);
        SpawnEdge(allNodes[5], allNodes[6], green);
        
        mutateLevel(level, mutationsPerLevel);
    }

    private List<WireColor> findMissingOutgoingColors(DFANode node)
    {
        var existingOutgoingColorLabels = new HashSet<string>(node.edges.Where(x => x.parent == node).Select(x => x.GetColorStr()));
        return allWireColors.Where(x => !existingOutgoingColorLabels.Contains(x.label)).ToList();
    }

    /**
    * This method does the mutations per the level
    * 
    * @param level - the current level
    * 
    * @param mutationsPerLevel - the amount of mutations needed per level
    */
    public void mutateLevel(int level, int mutationsPerLevel)
    {
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

        for (int i = 0; i < level * mutationsPerLevel; i++)
        {
            int mutationType = UnityEngine.Random.Range(0, 3);
            if (mutationType == 0)
            //Create new edge
            {
                var parentNode = allNodes[UnityEngine.Random.Range(0, allNodes.Count)];
                var childNode = parentNode;

                //Remove the while loop to enable recursive edges (edge going from a node to itself)
                while (childNode == parentNode)
                {
                    childNode = allNodes[UnityEngine.Random.Range(0, allNodes.Count)];
                }

                var missingColors = findMissingOutgoingColors(parentNode);

                if (missingColors.Count > 0)
                {
                    var newColor = missingColors[UnityEngine.Random.Range(0, missingColors.Count)];
                    SpawnEdge(parentNode, childNode, newColor);
                }
                else
                {
                    //Retry another mutation
                    i--;
                }
            }
            else if(mutationType == 1) //split a current edge
            {
                List<DFAEdge> outEdges;
                DFANode parentNode;
                do
                {
                    parentNode = allNodes[UnityEngine.Random.Range(0, allNodes.Count)];
                    outEdges = parentNode.edges.Where((other) => other.parent == parentNode).ToList();
                } while (outEdges.Count == 0);
                
                var edgeToSplit = outEdges[UnityEngine.Random.Range(0, outEdges.Count)];
                
                var childNode = edgeToSplit.child;
                var edgeColor = edgeToSplit.GetColorStr();



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

                var newNode = SpawnNode(origin + xOffset * x + yOffset * y);

                //spawn an edge from parent to new
                SpawnEdge(parentNode, newNode, allWireColors.First(wc => wc.label == edgeColor));
                //spawn edge from new node to child
                var wireColor = allWireColors[UnityEngine.Random.Range(0, allWireColors.Length)];

                SpawnEdge(newNode, childNode, wireColor);
                //remove the edge to split

                allEdges.Remove(edgeToSplit);
                parentNode.edges.Remove(edgeToSplit);
                childNode.edges.Remove(edgeToSplit);
                Destroy(edgeToSplit.gameObject);

            }
            else
            {
                var randNode = allNodes[UnityEngine.Random.Range(0, allNodes.Count)];
                var outgoingEdges = randNode.edges.Where(x => x.parent == randNode).ToList();
                if(outgoingEdges.Count > 0)
                {
                    var randEdge = outgoingEdges[UnityEngine.Random.Range(0, outgoingEdges.Count)];

                    var missingColors = findMissingOutgoingColors(randNode);

                    if (missingColors.Count > 0)
                    {
                        var newColor = missingColors[UnityEngine.Random.Range(0, missingColors.Count)];
                        randEdge.SetColor(newColor);
                    }
                    else
                    {
                        // Retry another mutation
                        i--;
                    }
                }
                else
                {
                    i--;
                }
            }
        }
    }
}

