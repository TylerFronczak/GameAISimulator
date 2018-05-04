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
    public List<Agent> agents;
    private ObjectPool<Agent> pooledAgents;

    [SerializeField] GameObject agentPrefab;

    private CellGrid cellGrid;
    private Pathfinder pathfinder;

    private GameObject agentsGameObject;
    private int numAgentsCreated;

    public void Initialize(CellGrid cellGrid, Pathfinder pathfinder)
    {
        this.cellGrid = cellGrid;
        this.pathfinder = pathfinder;

        agentsGameObject = new GameObject("Agents");
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

    #region EventSystem
    private void AddListeners()
    {
        EventManager.StartListening(CustomEventType.AgentDeath, AgentDeath);
        EventManager.StartListening(CustomEventType.AgentSelection, AgentSelection);
    }
    private void RemoveListeners()
    {
        EventManager.StopListening(CustomEventType.AgentDeath, AgentDeath);
        EventManager.StopListening(CustomEventType.AgentSelection, AgentSelection);
    }

    private void AgentDeath<Object>(Object agentObject)
    {
        Agent agent = agentObject as Agent;
        agents.Remove(agent);
        pooledAgents.Add(agent);
    }

    private void AgentSelection<Object>(Object agentObject)
    {
        Agent agent = agentObject as Agent;
        selectedAgent.Deselect();
        selectedAgent = agent;
        selectedAgent.Select();
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
