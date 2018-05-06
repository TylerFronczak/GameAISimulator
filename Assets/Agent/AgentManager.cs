//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using GameAISimulator.Enums;
using BehaviorTreeSystem;

public class AgentManager : MonoBehaviour
{
    public bool isSelectionEnabled;
    public Agent selectedAgent;
    private float agentSelectionDistanceThreshold = 1f;

    public List<Agent> agents;
    private ObjectPool<Agent> pooledAgents;

    [SerializeField] GameObject agentPrefab;

    private CellGrid cellGrid;
    private Pathfinder pathfinder;

    [SerializeField] GameObject agentsGameObject;
    private int numAgentsCreated;

    public void Initialize(CellGrid cellGrid, Pathfinder pathfinder)
    {
        this.cellGrid = cellGrid;
        this.pathfinder = pathfinder;

        pooledAgents = new ObjectPool<Agent>();
        pooledAgents.SetDelegate(CreateAgent);
    }

    public void UpdateAllAgents_Movement()
    {
        for (int i = 0; i < agents.Count; i++)
        {
            agents[i].UpdateAgent_Movement();
        }
    }

    public void UpdateAllAgents_Behavior()
    {
        for (int i = 0; i < agents.Count; i++)
        {
            agents[i].UpdateAgent_Behavior();
        }
    }

    private BehaviorTree GetBaseBehaviorTree(Agent agent)
    {
        BehaviorTree behaviorTree;
        behaviorTree = new AgentBehaviorTreeBuilder(new Selector(), agent)
            .Sequence("SeekFood")
                .Action(ActionType.AssignTarget_Food_ClosestInView)
                .Action(ActionType.MoveToTarget)
                .Action(ActionType.Eat)
                .Action(ActionType.ChangeColor_Passive)
            .End()
            .Sequence("Hunt")
                .Condition(ConditionType.IsStatus_Starving)
                .Action(ActionType.ChangeColor_Aggressive)
                .Action(ActionType.AssignTarget_Agent_ClosestInView)
                .Action(ActionType.SteerToTarget)
                .Action(ActionType.AttackTarget)
            .End()
            .Sequence("Wander")
                .Action(ActionType.Wander)
            .End()
            .Build();

        return behaviorTree;
    }

    public void CreateAgents(int numAgents)
    {
        bool isAbleToPlace;

        for (int i = 0; i < numAgents; i++)
        {
            Agent newAgent = pooledAgents.GetPoolObject();

            isAbleToPlace = false;
            while (isAbleToPlace == false)
            {
                isAbleToPlace = newAgent.PlaceOnGrid(cellGrid.GetRandomCell(0));
            }
        }
    }
    private Agent CreateAgent()
    {
        Agent agent = Instantiate(agentPrefab).GetComponent<Agent>();
        numAgentsCreated++;
        agent.agentName = string.Format("Agent #{0}", numAgentsCreated.ToString());
        agent.transform.parent = agentsGameObject.transform;
        agent.gameObject.name = agent.agentName;

        BehaviorTree behaviorTree = GetBaseBehaviorTree(agent);
        agent.Initialize(cellGrid, behaviorTree, pathfinder);
        agents.Add(agent);

        return agent;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            KillRandomAgent();
        }
    }
    private void KillRandomAgent()
    {
        int randomInt = Random.Range(0, agents.Count + 1);
        agents[randomInt].Death();
    }

    private void OnApplicationQuit()
    {
        foreach (Agent agent in agents)
        {
            Destroy(agent);
        }
    }

    private void SelectAgent(Agent agent)
    {
        selectedAgent.Deselect();
        selectedAgent = agent;
        selectedAgent.Select();
        EventManager.TriggerEvent(CustomEventType.AgentSelection, selectedAgent);
    }

    #region EventSystem
    private void AddListeners()
    {
        EventManager.StartListening(CustomEventType.AgentDeath, OnAgentDeath);
        EventManager.StartListening(CustomEventType.ClickedCell, OnClickedCell);
    }
    private void RemoveListeners()
    {
        EventManager.StopListening(CustomEventType.AgentDeath, OnAgentDeath);
        EventManager.StopListening(CustomEventType.ClickedCell, OnClickedCell);
    }

    private void OnAgentDeath<Object>(Object agentObject)
    {
        Agent agent = agentObject as Agent;
        agents.Remove(agent);
        pooledAgents.Add(agent);
    }

    private void OnClickedCell<Object>(Object cellObject)
    {
        Cell cell = cellObject as Cell;

        if (isSelectionEnabled)
        {
            Agent agent = cell.obstacle as Agent;

            if (agent != null)
            {
                SelectAgent(agent);
            }
            else
            {
                float closestDistance = Mathf.Infinity;
                Agent closestAgent = null;
                foreach (Cell neighbor in cell.Neighbors)
                {
                    agent = neighbor.obstacle as Agent;
                    if (agent != null)
                    {
                        float distance = Vector3.Distance(cell.transform.position, agent.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestAgent = agent;
                        }
                    }

                }

                if (closestAgent != null)
                {
                    SelectAgent(closestAgent);
                }
            }
        }
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

    public void ToggleFOV(bool isEnabled)
    {
        foreach (Agent agent in agents)
        {
            agent.fieldOfView.IsVisualizingFOV = isEnabled;
        }
    }
}
