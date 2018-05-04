//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;
using BehaviorTreeSystem.Enums;
using GameAISimulator;
using GameAISimulator.Enums;

public class MoveToTarget : AgentAction
{
    bool isTerminatingEarly;
    Vector3 startPosition;
    float timeSinceLastStallCheck;

    public MoveToTarget(Agent agentPerformingAction) : base(agentPerformingAction)
    {

    }

    protected override void OnInitialize()
    {
        //if (Vector3.Distance(agent.target.transform.position, agent.transform.position) <= 1)
        //{
        //    isTerminatingEarly = true;
        //}
        //else
        //{
        //    agent.movementController.PathToCell(agent.target.transform.position);
        //}
        agent.movementController.PathToCell(agent.target.transform.position);
        agent.movementMode = MovementMode.Pathing;
        timeSinceLastStallCheck = 0f;
        startPosition = agent.transform.position;
    }

    protected override BehaviorStatus OnUpdate()
    {
        //Debug.Log(string.Format("{0}: MoveToTarget.", agent.agentName));

        if (timeSinceLastStallCheck >= 1f)
        {
            if (Vector3.Distance(startPosition, agent.transform.position) < 0.1f)
            {
                //Debug.LogWarning(string.Format("{0}: Failure, movement has stalled.", agent.agentName));
                return BehaviorStatus.Failure;
            }
            else
            {
                timeSinceLastStallCheck = 0f;
                startPosition = agent.transform.position;
            }
        }
        else
        {
            timeSinceLastStallCheck += SimulationData.realSecondsBetweenAgentBehaviorUpdates;
        }


        //if (isTerminatingEarly)
        //{
        //    Debug.Log("Already at location, so no need to move.");
        //    agent.movementMode = MovementMode.None;
        //    return BehaviorStatus.Success;
        //}

        if (agent.target == null)
        {
            //Debug.LogWarning("No longer has valid move target, so stop pathing.");
            agent.movementController.Stop();
            return BehaviorStatus.Failure;
        }
        else if (Vector3.Distance(agent.target.transform.position, agent.transform.position) <= .75f)
        {
            agent.movementController.Stop();
            return BehaviorStatus.Success;
        }
        else
        {
            return BehaviorStatus.Running;
        }

        //if (agent.movementController.isPathing)
        //{
        //    return BehaviorStatus.Running;
        //}

        //return BehaviorStatus.Success;
    }
}
