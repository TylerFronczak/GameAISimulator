//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;
using UnityEngine.UI;
using GameAISimulator;
using GameAISimulator.Enums;
using BehaviorTreeSystem;

public class Agent : MonoBehaviour, IGridObject, IDamageable, IPoolObject
{
    public MovementController movementController;
    public PhysicsObject physicsObject;
    public Stats stats;
    public SimpleVehicleModel simpleVehicleModel;
    public FieldOfView fieldOfView;
    public AgentData data;
    public InfluenceSource influenceSource;

    public string agentName;

    private Vector3 uiOffsetY = new Vector3(0f, 0.5f, 0f);

    private float minutesPassedInSimulatedDaySinceLastUpdate;

    AgentBehavior currentBehavior;

    public MovementMode movementMode = MovementMode.None;

    public GameObject target;

    private BehaviorTree behaviorTree;

    [SerializeField] Image selectionIndicator;

    public void Initialize(CellGrid cellGrid, BehaviorTree agentBehaviorTree, Pathfinder pathfinder)
    {
        CellGrid = cellGrid;
        behaviorTree = agentBehaviorTree;

        data.LoadData();
        stats.Initialize(this);

        simpleVehicleModel.Initialize();
        movementController.Initialize(cellGrid, pathfinder);

        movementController.EnableAllPathingImprovements();
        //movementController.Toggle_CanJump(true);
        DisableAllMovementVisualization();
    }

    public void Initialize_PathfindingAgent(CellGrid cellGrid, Pathfinder pathfinder)
    {
        CellGrid = cellGrid;

        data.LoadData();
        stats.Initialize(this);

        simpleVehicleModel.Initialize();
        movementController.Initialize(cellGrid, pathfinder);
        movementController.IsVisualizingPath = true;

        //movementController.EnableAllPathingImprovements();
        //movementController.Toggle_CanJump(true);
        //DisableAllMovementVisualization();
    }

    void DisableAllMovementVisualization()
    {
        movementController.IsVisualizingPath = false;
        physicsObject.IsDrawingForces = false;
        simpleVehicleModel.ToggleVisualization_Steering(false);
        simpleVehicleModel.ToggleVisualization_Velocity(false);
        simpleVehicleModel.IsVisualizingCurrentBehavior = false;
    }

    public void UpdateAgent_Movement()
    {
        if (movementMode == MovementMode.Pathing)
        {
            movementController.FixedUpdate_MovementController();
        }
        else if (movementMode == MovementMode.Steering)
        {
            simpleVehicleModel.UpdateSteering();
        }

        Cell = CellGrid.GetCell(transform.position);
    }

    public void UpdateAgent_Behavior()
    {
        //if (isAlert)
        //{
        //    alertIcon.transform.position = transform.position + uiOffsetY;
        //    alertTimer += SimulationData.realSecondsBetweenAgentBehaviorUpdates;
        //    if (alertTimer >= alertDuration)
        //    {
        //        isAlert = false;
        //        alertIcon.enabled = false;
        //    }
        //}

        stats.ageInDays += SimulationData.simDaysBetweenBehaviorUpdates;

        totalEnergyExpenditurePerMinute = stats.ComputeTotalEnergyExpenditurePerMinute();
        float energyBurnedSinceLastUpdate = totalEnergyExpenditurePerMinute * SimulationData.minutesPassedInSimDayBetweenBehaviorUpdates;
        //Debug.Log(agentName + " has burned " + totalEnergyExpenditurePerMinute + " in a minute, for a total of " + energyBurnedSinceLastUpdate + " since last update, which was " + SimulationData.minutesPassedInSimDayBetweenBehaviorUpdates + "minutes in the simulation");
        stats.ModifyHunger(totalEnergyExpenditurePerMinute * SimulationData.minutesPassedInSimDayBetweenBehaviorUpdates);
        
        behaviorTree.Tick();
    }

    public float totalEnergyExpenditurePerMinute;

    #region IGridObject
    public GridObjectType Type { get { return GridObjectType.Agent; } }

    public CellGrid CellGrid { get; set; }
    private Cell cell;
    public Cell Cell
    {
        get
        {
            return cell;
        }
        set
        {
            if (value != cell)
            {
                cell.DeregisterAgent(this);
                cell = value;
                cell.RegisterAgent(this);
            }
        }
    }

    /// <summary> Will return false if unable to place on grid. </summary>
    public bool PlaceOnGrid(int column, int row)
    {
        Cell possibleCell = CellGrid.GetCell(column, row);
        return PlaceOnGrid(possibleCell);
    }
    public bool PlaceOnGrid(Cell possibleCell)
    {
        if (possibleCell.obstacle != null)
        {
            Debug.Log("Agent cannot be placed on an already occupied cell.");
            return false;
        }

        cell = possibleCell;
        cell.obstacle = this;
        transform.position = cell.transform.position;
        return true;
    }

    public void RemoveFromGrid()
    {
        if (cell != null)
        {
            cell.obstacle = null;
        }
    }

    public void OnDestroy()
    {
        RemoveFromGrid();
    }
    #endregion

    #region IDamageable
    public float Health {
        get { return stats.Health; }
    }

    public void ReceiveDamage(float damage)
    {
        stats.ModifyCurrentHealth(-damage);
    }

    public void Death()
    {
        isDead = true;
        influenceSource.enabled = false;
        EventManager.TriggerEvent(CustomEventType.AgentDeath, this);
    }
    #endregion

    private bool isDead;

    #region IPoolObject
    public void ResetForPool()
    {
        gameObject.SetActive(false);
    }

    public void OnPoolRemoval()
    {
        isDead = false;
        influenceSource.enabled = true;
        gameObject.SetActive(true);
    }
    #endregion

    public void Select()
    {
        selectionIndicator.enabled = true;
    }

    public void Deselect()
    {
        selectionIndicator.enabled = false;
    }
}
