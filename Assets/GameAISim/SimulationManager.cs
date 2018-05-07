//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameAISimulator;
using GameAISimulator.Enums;

public class SimulationManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] int columns;
    [SerializeField] int rows;
    [SerializeField] Canvas gridCanvas;
    [SerializeField] CellGrid cellGrid;

    [Header("NodeGraph")]
    [SerializeField] NodeGraph nodeGraph;

    [Header("LevelEditor")]
    [SerializeField] LevelEditor levelEditor;

    [Header("Region Manager")]
    [SerializeField] RegionManager regionManager;
    [SerializeField] LineRenderer portalLineRenderer;

    [Header("Pathfinder")]
    [SerializeField] Pathfinder pathfinder;

    [Header("Influence Map Manager")]
    [SerializeField] InfluenceMapManager influenceMapManager;

    [Header("Boid Manager")]
    [SerializeField] BoidManager boidManager;

    [Header("Biome")]
    [SerializeField] Ecosystem biome;

    [Header("Editor Debugging")]
    [SerializeField] bool isDrawNodes;
    float debugSphereRadius = CellMetrics.offset / 4f;

    [Header("Selected Agent")]
    public Agent selectedAgent;

    [Header("Agent Manager")]
    [SerializeField] AgentManager agentManager;

    [Header("CustomCamera")]
    [SerializeField] CustomCamera customCamera;

    [Header("SimulationModes")]
    private SimulationMode simulationMode;
    [SerializeField] Agent pathfindingAgent;

    void Awake()
    {
        cellGrid.Initialize(columns, rows, gridCanvas);
        nodeGraph.Initialize(cellGrid.Cells, columns, rows);
        HandleIntents_Terrain();
        regionManager.Initialize(cellGrid, gridCanvas);

        pathfinder.Initialize();
        influenceMapManager.Initialize(pathfinder, regionManager.Regions);
        agentManager.Initialize(cellGrid, pathfinder);
        biome.Initialize(cellGrid);
        HandleIntents_SimulationMode();

        customCamera.Initialize(cellGrid, true);
        customCamera.SetClamp(0f, (cellGrid.Columns - 1) * CellMetrics.offset, 0f, (cellGrid.Rows - 1) * CellMetrics.offset);

        //CloseAllPanels();
    }

    float updateTimer;

    private void FixedUpdate()
    {
        if (simulationMode == SimulationMode.Ecosystem)
        {
            updateTimer += Time.fixedDeltaTime;
            if (updateTimer >= SimulationData.realSecondsBetweenAgentBehaviorUpdates)
            {
                updateTimer = 0f;
                agentManager.UpdateAllAgents_Behavior();
                if (isSelectedAgentPanelActive)
                {
                    UpdateAgentText();
                }

                biome.UpdateBiome(SimulationData.realSecondsBetweenAgentBehaviorUpdates);
            }
        }

        agentManager.UpdateAllAgents_Movement();
    }

    private bool isAcceptingPathfindingInput;
    private bool isAcceptingSteeringInput;

    private void LoadMode(int mode)
    {
        if (mode == 0)
        {
            simulationMode = SimulationMode.Pathfinding;

            isAcceptingPathfindingInput = true;
            pathfinder.SelectHeuristicWeight(0);
            pathfindingAgent.gameObject.SetActive(true);
            pathfindingAgent.Initialize_PathfindingAgent(cellGrid, pathfinder);
            pathfindingAgent.PlaceOnGrid(cellGrid.GetCell(12, 12));
            pathfindingAgent.movementMode = MovementMode.Pathing;
            agentManager.agents.Add(pathfindingAgent);

            pathfinderButtonObject.SetActive(true);
            TogglePanel_Pathfinder(true);
        }
        else if (mode == 1)
        {
            simulationMode = SimulationMode.Steering;
            isAcceptingSteeringInput = true;
            boidManager.gameObject.SetActive(true);
            boidManager.Initialize(cellGrid);
            //boidManager.ToggleBoids();
            boidManager.CreateBoids(15);
            boidManager.ToggleVisualization_Steering(true);
            boidManager.ToggleVisualization_Velocity(true);
            boidManager.ChangeMode_SeekAndSeperation();

            boidManagerButtonObject.SetActive(true);
            TogglePanel_BoidManager(true);
        }
        else if (mode == 2)
        {
            simulationMode = SimulationMode.Ecosystem;

            biome.CreatePlants(100);
            biome.StartSpawningPlants(0.25f);

            agentManager.CreateAgents(5);
            agentManager.isSelectionEnabled = true;

            ecosystemManagerButtonObject.SetActive(true);
            TogglePanel_EcosystemManager(true);
            influenceMapManager.TogglePopulationMapOptions(false);
        }
    }

    void HandleIntents()
    {
        if (IntentManager.Instance != null)
        {
            IntentManager intentManager = IntentManager.Instance;
            List<Intent> intents = intentManager.Intents;
            for (int i = 0; i < intents.Count; i++)
            {
                switch (intents[i].intentType)
                {
                    case IntentType.LoadTerrain:
                        levelEditor.Load(intents[i].intentString, intents[i].intentBool);
                        break;
                    case IntentType.LoadMode:
                        LoadMode(intents[i].intentInt);
                        break;
                }
            }
        }
    }
    void HandleIntents_Terrain()
    {
        if (IntentManager.Instance != null)
        {
            IntentManager intentManager = IntentManager.Instance;
            List<Intent> intents = intentManager.Intents;
            for (int i = 0; i < intents.Count; i++)
            {
                if (intents[i].intentType == IntentType.LoadTerrain)
                {
                    levelEditor.Load(intents[i].intentString, intents[i].intentBool);
                }
            }
        }
    }
    void HandleIntents_SimulationMode()
    {
        if (IntentManager.Instance != null)
        {
            IntentManager intentManager = IntentManager.Instance;
            List<Intent> intents = intentManager.Intents;
            for (int i = 0; i < intents.Count; i++)
            {
                if (intents[i].intentType == IntentType.LoadMode)
                {
                    LoadMode(intents[i].intentInt);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (isDrawNodes)
        {
            int debugNumEdgeLinesDrawn = 0;
            int debugNumEdgeSpheres = 0;
            for (int i = 0; i < nodeGraph.Nodes.Count; i++)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(nodeGraph.Nodes[i].position, debugSphereRadius);

                for (int e = 0; e < nodeGraph.Nodes[i].Edges.Count; e++)
                {
                    Direction direction = nodeGraph.Nodes[i].Edges[e].DirectionFromNodeOne;
                    if (direction == Direction.W || direction == Direction.SW || direction == Direction.S || direction == Direction.SE)
                    {
                        debugNumEdgeLinesDrawn++;
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(
                            nodeGraph.Nodes[i].position,
                            nodeGraph.Nodes[i].Edges[e].Position
                            );

                        // Is this nodeOne? If so, a debug sphere should mark the edge.
                        // Otherwise, drawing a debug sphere would be a duplicate.
                        if (nodeGraph.Nodes[i] == nodeGraph.Nodes[i].Edges[e].NodeOne)
                        {
                            debugNumEdgeSpheres++;
                            if (nodeGraph.Nodes[i].Edges[e].EdgeType == EdgeType.Inaccessible)
                            {
                                Gizmos.color = Color.red;
                                //Gizmos.DrawSphere(nodes[i].edges[e].edgePosition, debugSphereRadius / 2);
                            }
                            else if (nodeGraph.Nodes[i].Edges[e].EdgeType == EdgeType.Elevated)
                            {
                                Gizmos.color = Color.green;
                            }
                            else if (nodeGraph.Nodes[i].Edges[e].EdgeType == EdgeType.SlopeConnection)
                            {
                                Gizmos.color = Color.cyan;
                            }
                            else
                            {
                                Gizmos.color = Color.black;
                            }

                            Gizmos.DrawSphere(nodeGraph.Nodes[i].Edges[e].Position, debugSphereRadius / 2);
                        }
                    }
                }
            }
            //Debug.Log("Node Edge Lines Drawn: " + debugNumEdgeLinesDrawn + "!");
            //Debug.Log("Node Edge Spheres Drawn: " + debugNumEdgeSpheres + "!");
        }

        if (isVisualizingRegions)
        {
            for (int i = 0; i < regionManager.Regions.Count; i++)
            {
                for (int p = 0; p < regionManager.Regions[i].portals.Count; p++)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawSphere(regionManager.Regions[i].portals[p].position, debugSphereRadius);
                }
            }
        }
    }

    bool isVisualizingSearch;

    public void ToggleSearchVisualization(bool isEnabled)
    {
        isVisualizingSearch = isEnabled;

        for (int i = 0; i < nodeGraph.Nodes.Count; i++)
        {
            nodeGraph.Nodes[i].IsVisualizingSearch = isVisualizingSearch;
        }
    }

    bool isVisualizingRegions;

    public void ToggleRegionVisualization(bool isEnabled)
    {
        // TEMP!!!!!
        if (!regionManager.IsInitialized)
        {
            regionManager.Initialize(cellGrid, gridCanvas);
        }

        isVisualizingRegions = isEnabled;

        if (isVisualizingRegions)
        {
            Vector3 lineRendererOffset = new Vector3(0f, 0.1f, 0f);

            for (int i = 0; i < regionManager.Regions.Count; i++)
            {
                LineRenderer lineRenderer = regionManager.Regions[i].LineRenderer;
                int positionsCount = regionManager.Regions[i].portals.Count * 2;
                Vector3[] lineRendererPositions = new Vector3[positionsCount];

                for (int p = 0; p < regionManager.Regions[i].portals.Count; p++)
                {
                    lineRendererPositions[p * 2] = regionManager.Regions[i].centerPosition + lineRendererOffset;
                    lineRendererPositions[p * 2 + 1] = regionManager.Regions[i].portals[p].position + lineRendererOffset;
                }

                lineRenderer.positionCount = lineRendererPositions.Length;
                lineRenderer.SetPositions(lineRendererPositions);
                lineRenderer.enabled = true;
            }
        }

        for (int i = 0; i < regionManager.Regions.Count; i++)
        {
            regionManager.Regions[i].ToggleUI(isVisualizingRegions);
        }
    }

    public void ToggleRegionVisualizationWithoutLineRenderer(bool isEnabled)
    {
        isVisualizingRegions = isEnabled;

        for (int i = 0; i < regionManager.Regions.Count; i++)
        {
            regionManager.Regions[i].LineRenderer.enabled = !isEnabled;
            regionManager.Regions[i].ToggleUI(isVisualizingRegions);
        }
    }

    void PrintCellRegions()
    {
        for (int i = 0; i < cellGrid.Cells.Length; i++)
        {
            Debug.Log("Cell coordinates: " + cellGrid.Cells[i].Coordinates.ToString() + " Region: " + cellGrid.Cells[i].regionID);
        }
    }

    void PrintCellEdgeDirections()
    {
        for (int i = 0; i < cellGrid.Cells.Length; i++)
        {
            string debugString = string.Empty;

            for (int e = 0; e < cellGrid.Cells[i].node.Edges.Count; e++)
            {
                debugString += " " + cellGrid.Cells[i].node.Edges[e].DirectionFromNodeOne.ToString();
            }

            Debug.Log("Cell coordinates: " + cellGrid.Cells[i].Coordinates.ToString() + " Edge Directions: " + debugString);
        }
        
    }

    #region EventSystem
    private void AddListeners()
    {
        EventManager.StartListening(CustomEventType.ClickedCell, ClickedCell);
        EventManager.StartListening(CustomEventType.AgentSelection, OnAgentSelection);
    }
    private void RemoveListeners()
    {
        EventManager.StopListening(CustomEventType.ClickedCell, ClickedCell);
        EventManager.StopListening(CustomEventType.AgentSelection, OnAgentSelection);
    }

    void ClickedCell<Object>(Object clickedCell)
    {
        Cell cell = clickedCell as Cell;

        if (isAcceptingPathfindingInput)
        {
            if (pathfindingAgent == null)
            {
                Debug.Log("No agent is currently selected.");
            }
            else
            {
                pathfindingAgent.movementController.PathToCell(cell);
            }
        }
        else if (isAcceptingSteeringInput)
        {
            if (cell.Elevation == 0)
            {
                boidManager.AssignSteeringTarget(cell);
            }
        }
        else
        {
            if (cell.obstacle != null)
            {
                Agent agent = cell.obstacle as Agent;
                Debug.Log(agent.name);
            }
            else if (cell.obstacle == null)
            {
                Debug.Log("There is no grid object");
            }
        }
    }

    void OnAgentSelection<Object>(Object agentObject)
    {
        selectedAgent = agentObject as Agent;
        UpdateAgentText();
    }

    private void OnEnable()
    {
        AddListeners();
    }

    private void OnDisable()
    {
        RemoveListeners();   
    }
    #endregion

    #region UI
    [Header("UI")]
    [SerializeField] GameObject pathfinderButtonObject;
    [SerializeField] GameObject pathfinderPanel;
    bool isPathfinderPanelActive;

    [SerializeField] GameObject boidManagerButtonObject;
    [SerializeField] GameObject boidManagerPanel;
    bool isBoidManagerPanelActive;

    [SerializeField] GameObject ecosystemManagerButtonObject;
    [SerializeField] GameObject ecosystemManagerPanel;

    [SerializeField] GameObject selectedAgentPanel;
    bool isSelectedAgentPanelActive;

    public void TogglePanel_Pathfinder()
    {
        isPathfinderPanelActive = !isPathfinderPanelActive;
        TogglePanel_Pathfinder(isPathfinderPanelActive);
    }
    public void TogglePanel_Pathfinder(bool isEnabled)
    {
        if (isEnabled)
        {
            CloseAllPanels();
        }

        //if (pathfindingAgent != null)
        //{
        //    isAcceptingPathfindingInput = isEnabled;
        //}

        pathfinderPanel.SetActive(isEnabled);
        isPathfinderPanelActive = isEnabled;
    }

    public void TogglePanel_BoidManager()
    {
        isBoidManagerPanelActive = !isBoidManagerPanelActive;
        TogglePanel_BoidManager(isBoidManagerPanelActive);
    }
    public void TogglePanel_BoidManager(bool isEnabled)
    {
        if (isEnabled)
        {
            CloseAllPanels();
        }

        boidManagerPanel.SetActive(isEnabled);
        isBoidManagerPanelActive = isEnabled;
    }

    private bool isEcosystemPanelEnabled;
    public void TogglePanel_EcosystemManager()
    {
        isEcosystemPanelEnabled = !isEcosystemPanelEnabled;
        TogglePanel_EcosystemManager(isEcosystemPanelEnabled);
    }
    public void TogglePanel_EcosystemManager(bool isEnabled)
    {
        isEcosystemPanelEnabled = isEnabled;
        if (isEcosystemPanelEnabled)
        {
            CloseAllPanels();
        }

        ecosystemManagerPanel.SetActive(isEcosystemPanelEnabled);
    }

    [SerializeField] Text healthText;
    [SerializeField] Text ageText;
    [SerializeField] Text heightText;
    [SerializeField] Text hungerText;
    [SerializeField] Text weightText;
    [SerializeField] Text bmiText;
    [SerializeField] Text teeText;

    public void TogglePanel_SelectedAgent()
    {
        isSelectedAgentPanelActive = !isSelectedAgentPanelActive;
        TogglePanel_SelectedAgent(isSelectedAgentPanelActive);
    }
    public void TogglePanel_SelectedAgent(bool isEnabled)
    {
        if (isEnabled)
        {
            CloseAllPanels();
            UpdateAgentText();
        }

        if (pathfindingAgent != null)
        {
            isAcceptingPathfindingInput = isEnabled;
        }

        selectedAgentPanel.SetActive(isEnabled);
        isSelectedAgentPanelActive = isEnabled;
    }


    void UpdateAgentText()
    {
        healthText.text = selectedAgent.stats.GetFormattedHealth(false);
        weightText.text = selectedAgent.stats.GetFormattedWeight();
        bmiText.text = selectedAgent.stats.GetFormattedBMI();
        hungerText.text = selectedAgent.stats.GetFormattedHunger();
        ageText.text = selectedAgent.stats.GetFormattedAge();
        heightText.text = selectedAgent.stats.GetFormattedHeight();
        teeText.text = string.Format("TEE: {0:#.00} kcal/min", selectedAgent.totalEnergyExpenditurePerMinute);
    }

    public void CloseAllPanels()
    {
        pathfinderPanel.SetActive(false);
        isPathfinderPanelActive = false;

        boidManagerPanel.SetActive(false);
        isBoidManagerPanelActive = false;

        ecosystemManagerPanel.SetActive(false);

        selectedAgentPanel.SetActive(false);
        isSelectedAgentPanelActive = false;
    }
    #endregion
}
